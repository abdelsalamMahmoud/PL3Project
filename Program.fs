open System
open System.Windows.Forms
open System.Drawing

// Define a new function that will be called from the GUI
let showMessage () =
    MessageBox.Show("Button clicked!") |> ignore

// Another function that changes the label text
let changeLabelText (label: Label) =
    label.Text <- "Text has been changed!"

[<EntryPoint>]
let main argv =
    // Create the form
    let form = new Form(Text = "F# Windows Forms Example", Width = 800, Height = 600)
    
    // Create a label
    let label = new Label(Text = "Hello, Windows Forms!", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter)
    
    // Create a button
    let button = new Button(Text = "Click Me", Dock = DockStyle.Top)

    // Button click event to show a message
    button.Click.Add(fun _ -> showMessage())

    // Button click event to change label text
    button.Click.Add(fun _ -> changeLabelText label)

    // Add the label and button to the form
    form.Controls.Add(label)
    form.Controls.Add(button)

    // Run the application
    Application.Run(form)

    0