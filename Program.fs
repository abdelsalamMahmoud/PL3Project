open System
open System.Windows.Forms
open System.Drawing

// Define student type
type Student = { ID: int; Name: string; Grades: int list }

open MySql.Data.MySqlClient

// String الاتصال
let connectionString = "Server=localhost;Database=StudentDB;User=root;Password=1234;"





// Static student data
let students = [
    { ID = 1; Name = "Alice"; Grades = [85; 90; 78] }
    { ID = 2; Name = "Bob"; Grades = [92; 88; 95] }
    { ID = 3; Name = "Charlie"; Grades = [70; 60; 80] }
    { ID = 4; Name = "Diana"; Grades = [88; 76; 94] }
]

// Calculate average grade
let calculateAverage grades =
    grades
    |> List.map float // Convert integers to floats
    |> List.average   // Calculate average
    |> int            // Convert the result back to an integer

// Calculate class statistics
let calculateClassStatistics () =
    let allGrades = students |> List.collect (fun s -> s.Grades)
    let average = calculateAverage allGrades
    let maxGrade = List.max allGrades
    let minGrade = List.min allGrades
    sprintf "Class Average: %d\nHighest Grade: %d\nLowest Grade: %d" average maxGrade minGrade





// إضافة طالب جديد
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



// جلب بيانات الطلبة
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
            let grades = reader.GetString("Grades").Split(',') |> Array.map float |> List.ofArray
            yield { ID = id; Name = name; Grades = grades } ]
    students



// تحديث بيانات طالب
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


// حذف طالب
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
    students
    |> List.iter (fun s -> listBox.Items.Add(sprintf "ID: %d, Name: %s" s.ID s.Name) |> ignore)

    // Create a TextBox for details
    let detailsBox = new TextBox(Multiline = true, ReadOnly = true, Dock = DockStyle.Fill)
    
    // Create a button to calculate class statistics
    let statsButton = new Button(Text = "Show Class Statistics", Dock = DockStyle.Top)

    // Event to display selected student's details
    listBox.SelectedIndexChanged.Add(fun _ ->
        if listBox.SelectedIndex >= 0 then
            let student = students.[listBox.SelectedIndex]
            let avg = calculateAverage student.Grades
            detailsBox.Text <- 
                sprintf "ID: %d\nName: %s\nGrades: %s\nAverage: %d" 
                    student.ID student.Name (String.Join(", ", student.Grades)) avg
    )

    // Event to calculate and display class statistics
    statsButton.Click.Add(fun _ ->
        detailsBox.Text <- calculateClassStatistics ()
    )
    
    // Add controls to the form
    form.Controls.Add(detailsBox)
    form.Controls.Add(listBox)
    form.Controls.Add(statsButton)
    
    // Run the application
    Application.Run(form)
    0