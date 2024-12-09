open System
open System.Windows.Forms
open System.Drawing
open MySql.Data.MySqlClient

// Define student type with grades as int list
type Student = { ID: int; Name: string; Grades: int list }

// Database connection string
let connectionString = "Server=localhost;Database=StudentDB;User=root;Password=abdelsalam30;"

// Function to fetch all students from the database
let getStudentsFromDB () =
    use connection = new MySqlConnection(connectionString)
    connection.Open()
    let query = "SELECT * FROM Students"
    use command = new MySqlCommand(query, connection)
    use reader = command.ExecuteReader()
    let students = 
        [ while reader.Read() do
            let id = reader.GetInt32("ID")
            let name = reader.GetString("Name")
            let grades = reader.GetString("Grades")
                          .Split(',')
                          |> Array.map (fun g -> int (float g))
                          |> List.ofArray
            yield { ID = id; Name = name; Grades = grades } ]
    students

// Calculate average grade
let calculateAverage grades =
    grades
    |> List.map float    
    |> List.average      
    |> int 

// Calculate class statistics
let calculateClassStatistics () =
    let allGrades = getStudentsFromDB () |> List.collect (fun s -> s.Grades)
    let average = calculateAverage allGrades
    let maxGrade = List.max allGrades
    let minGrade = List.min allGrades
    sprintf "Class Average: %d\nHighest Grade: %d\nLowest Grade: %d" average maxGrade minGrade

// Add a new student to the database
let addStudentToDB name grades =
    let gradesAsString = String.Join(",", grades |> List.map string)
    use connection = new MySqlConnection(connectionString)
    connection.Open()
    let query = "INSERT INTO Students (Name, Grades) VALUES (@name, @grades)"
    use command = new MySqlCommand(query, connection)
    command.Parameters.AddWithValue("@name", name) |> ignore
    command.Parameters.AddWithValue("@grades", gradesAsString) |> ignore
    command.ExecuteNonQuery() |> ignore
    printfn "تم إضافة الطالب: %s" name

// Update student information in the database
let updateStudentInDB id newName newGrades =
    let gradesAsString = String.Join(",", newGrades |> List.map string)
    use connection = new MySqlConnection(connectionString)
    connection.Open()
    let query = "UPDATE Students SET Name = @name, Grades = @grades WHERE ID = @id"
    use command = new MySqlCommand(query, connection)
    command.Parameters.AddWithValue("@id", id) |> ignore
    command.Parameters.AddWithValue("@name", newName) |> ignore
    command.Parameters.AddWithValue("@grades", gradesAsString) |> ignore
    command.ExecuteNonQuery() |> ignore
    printfn "تم تحديث بيانات الطالب ID: %d" id

// Delete student from the database
let deleteStudentFromDB id =
    use connection = new MySqlConnection(connectionString)
    connection.Open()
    let query = "DELETE FROM Students WHERE ID = @id"
    use command = new MySqlCommand(query, connection)
    command.Parameters.AddWithValue("@id", id) |> ignore
    command.ExecuteNonQuery() |> ignore
    printfn "تم حذف الطالب ID: %d" id

[<EntryPoint>]
let main argv =
    // Create the form
    let form = new Form(Text = "Student Grades Management System", Width = 800, Height = 600)
    
    // Create a ListBox to display students
    let listBox = new ListBox(Dock = DockStyle.Left, Width = 250)
    
    // Load students from database and display in ListBox
    let loadStudents () =
        listBox.Items.Clear()
        let students = getStudentsFromDB ()
        students
        |> List.iter (fun s -> listBox.Items.Add(sprintf "ID: %d, Name: %s" s.ID s.Name) |> ignore)

    // Load students initially
    loadStudents()

    // Create a TextBox for details
    let detailsBox = new TextBox(Multiline = true, ReadOnly = true, Dock = DockStyle.Fill)
    
    // Create a button to calculate class statistics
    let statsButton = new Button(Text = "Show Class Statistics", Dock = DockStyle.Top)

    // Event to display selected student's details
    listBox.SelectedIndexChanged.Add(fun _ -> 
        if listBox.SelectedIndex >= 0 then
            let student = getStudentsFromDB () |> List.item listBox.SelectedIndex
            let avg = calculateAverage student.Grades
            detailsBox.Text <- 
                sprintf "ID: %d\nName: %s\nGrades: %s\nAverage: %d" 
                    student.ID student.Name (String.Join(", ", student.Grades)) avg
    )

    // Event to calculate and display class statistics
    statsButton.Click.Add(fun _ -> 
        detailsBox.Text <- calculateClassStatistics ()
    )

    // Create a button to add a new student
    let addButton = new Button(Text = "Add New Student", Dock = DockStyle.Top)

    // Event to add a new student with individual grade inputs
    addButton.Click.Add(fun _ ->
        let addDialog = new Form(Text = "Add New Student", Width = 400, Height = 400)
        
        // Create input controls
        let nameLabel = new Label(Text = "Student Name:", Location = Point(10, 20))
        let nameInput = new TextBox(Location = Point(120, 20), Width = 250)
        
        // Create grade input fields with labels
        let gradeLabels = [1..5] |> List.map (fun i -> 
            new Label(Text = sprintf "Grade %d:" i, Location = Point(10, 50 + (i * 30))))
            
        let gradeInputs = [1..5] |> List.map (fun i -> 
            new TextBox(Location = Point(120, 50 + (i * 30)), Width = 100))
            
        // Add description label
        let descLabel = new Label(
            Text = "Enter grades (0-100). Leave empty if not applicable.",
            Location = Point(10, 220),
            Width = 350)
            
        let submitButton = new Button(Text = "Add Student", Location = Point(120, 250))
        
        // Add button click handler
        submitButton.Click.Add(fun _ ->
            try
                let name = nameInput.Text
                if String.IsNullOrWhiteSpace(name) then
                    MessageBox.Show("Please enter a student name") |> ignore
                else
                    let grades = gradeInputs 
                                |> List.map (fun input -> 
                                    if String.IsNullOrWhiteSpace(input.Text) then None
                                    else 
                                        let grade = int input.Text
                                        if grade >= 0 && grade <= 100 then Some grade
                                        else None)
                                |> List.choose id

                    if List.isEmpty grades then
                        MessageBox.Show("Please enter at least one valid grade (0-100)") |> ignore
                    else
                        addStudentToDB name grades
                        MessageBox.Show(sprintf "تم إضافة الطالب: %s" name) |> ignore
                        loadStudents()
                        addDialog.Close()
            with
            | ex -> MessageBox.Show("Please enter valid numeric grades between 0 and 100") |> ignore
        )
        
        // Add all controls to the form
        addDialog.Controls.AddRange(
            Array.concat [
                [| nameLabel :> Control; nameInput :> Control; descLabel :> Control; submitButton :> Control |]
                (gradeLabels |> List.map (fun x -> x :> Control) |> Array.ofList)
                (gradeInputs |> List.map (fun x -> x :> Control) |> Array.ofList)
            ])
        
        addDialog.ShowDialog() |> ignore
    )

    // Create a button to delete a student
    let deleteButton = new Button(Text = "Delete Selected Student", Dock = DockStyle.Top)

    // Event to delete the selected student
    deleteButton.Click.Add(fun _ ->
        if listBox.SelectedIndex >= 0 then
            let student = getStudentsFromDB () |> List.item listBox.SelectedIndex
            if MessageBox.Show(sprintf "Are you sure you want to delete %s?" student.Name, 
                             "Confirm Delete", MessageBoxButtons.YesNo) = DialogResult.Yes then
                deleteStudentFromDB student.ID
                MessageBox.Show(sprintf "تم حذف الطالب ID: %d" student.ID) |> ignore
                loadStudents()
    )

    // Create a button to update student details
    let updateButton = new Button(Text = "Update Selected Student", Dock = DockStyle.Top)

    // Event to update the selected student's details
    updateButton.Click.Add(fun _ ->
        if listBox.SelectedIndex >= 0 then
            let student = getStudentsFromDB () |> List.item listBox.SelectedIndex
            let updateDialog = new Form(Text = "Update Student", Width = 400, Height = 400)
            
            // Create input controls
            let nameLabel = new Label(Text = "New Name:", Location = Point(10, 20))
            let nameInput = new TextBox(Text = student.Name, Location = Point(120, 20), Width = 250)
            
            // Create grade input fields
            let gradeLabels = [1..5] |> List.map (fun i -> 
                new Label(Text = sprintf "Grade %d:" i, Location = Point(10, 50 + (i * 30))))
                
            let gradeInputs = [1..5] |> List.mapi (fun i _ -> 
                let input = new TextBox(Location = Point(120, 50 + (i * 30)), Width = 100)
                if i < student.Grades.Length then
                    input.Text <- student.Grades.[i].ToString()
                input)
                
            // Add description label
            let descLabel = new Label(
                Text = "Enter grades (0-100). Leave empty if not applicable.",
                Location = Point(10, 220),
                Width = 350)
                
            let submitButton = new Button(Text = "Update", Location = Point(120, 250))
            
            // Update button click handler
            submitButton.Click.Add(fun _ ->
                try
                    let newName = nameInput.Text
                    if String.IsNullOrWhiteSpace(newName) then
                        MessageBox.Show("Please enter a student name") |> ignore
                    else
                        let newGrades = gradeInputs 
                                      |> List.map (fun input -> 
                                          if String.IsNullOrWhiteSpace(input.Text) then None
                                          else 
                                              let grade = int input.Text
                                              if grade >= 0 && grade <= 100 then Some grade
                                              else None)
                                      |> List.choose id

                        if List.isEmpty newGrades then
                            MessageBox.Show("Please enter at least one valid grade (0-100)") |> ignore
                        else
                            updateStudentInDB student.ID newName newGrades
                            MessageBox.Show(sprintf "تم تحديث بيانات الطالب ID: %d" student.ID) |> ignore
                            loadStudents()
                            updateDialog.Close()
                with
                | ex -> MessageBox.Show("Please enter valid numeric grades between 0-100") |> ignore
            )
            
            // Add all controls to the form
            updateDialog.Controls.AddRange(
                Array.concat [
                    [| nameLabel :> Control; nameInput :> Control; descLabel :> Control; submitButton :> Control |]
                    (gradeLabels |> List.map (fun x -> x :> Control) |> Array.ofList)
                    (gradeInputs |> List.map (fun x -> x :> Control) |> Array.ofList)
                ])
            
            updateDialog.ShowDialog() |> ignore
    )
    
    // Add controls to the main form
    form.Controls.Add(detailsBox)
    form.Controls.Add(listBox)
    form.Controls.Add(statsButton)
    form.Controls.Add(addButton)
    form.Controls.Add(deleteButton)
    form.Controls.Add(updateButton)
    
    // Run the application
    Application.Run(form)
    0 
