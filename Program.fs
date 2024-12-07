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
                          |> Array.map (fun g -> int (float g)) // Convert to int after converting to float
                          |> List.ofArray
            yield { ID = id; Name = name; Grades = grades } ]
    students

// Calculate average grade
let calculateAverage grades =
    grades
    |> List.map float    // Convert integers to floats
    |> List.average      // Calculate average as float
    |> (fun avg -> int avg) // Convert float to int explicitly after averaging

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

// Build the GUI
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

    // Event to add a new student
    addButton.Click.Add(fun _ ->
        let name = "New Student" // Replace this with your input form or dialog box for name
        let grades = [90; 85; 88] // Replace this with user input for grades
        addStudentToDB name grades
        loadStudents() // Reload student list
    )

    // Create a button to delete a student
    let deleteButton = new Button(Text = "Delete Selected Student", Dock = DockStyle.Top)

    // Event to delete the selected student
    deleteButton.Click.Add(fun _ ->
        if listBox.SelectedIndex >= 0 then
            let student = getStudentsFromDB () |> List.item listBox.SelectedIndex
            deleteStudentFromDB student.ID
            loadStudents() // Reload student list
    )

    // Create a button to update student details
    let updateButton = new Button(Text = "Update Selected Student", Dock = DockStyle.Top)

    // Event to update the selected student's details
    updateButton.Click.Add(fun _ ->
        if listBox.SelectedIndex >= 0 then
            let student = getStudentsFromDB () |> List.item listBox.SelectedIndex
            let newName = "Updated Name" // Replace this with user input for new name
            let newGrades = [95; 90; 92] // Replace this with user input for new grades
            updateStudentInDB student.ID newName newGrades
            loadStudents() // Reload student list
    )
    
    // Add controls to the form
    form.Controls.Add(detailsBox)
    form.Controls.Add(listBox)
    form.Controls.Add(statsButton)
    form.Controls.Add(addButton)
    form.Controls.Add(deleteButton)
    form.Controls.Add(updateButton)
    
    // Run the application
    Application.Run(form)
    0
