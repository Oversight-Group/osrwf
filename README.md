# OsRwf

# The Idea

The library was developed by Amit (Amit_B) Barami to assist C# WinForms developers to make responsive Windows desktop apps.
WinForms enviroment is not responsive by default. This means that if I develop a desktop app using screen resolution higher than the user's screen resolution, he may not see all of the content of the window! This also can go the same way around, when developing with screen resolution lower than the user's screen resolution, the user may get a small app, which would be not user friendly and may be not readable.

I wanted to make something to cause the 'magic' happen, that using a single method, any developer can make his WinForms app responsive.
The code behind the magic is simple; the class first compares the given two screen resolutions, which are the design-time resolution and the run-time resolution. Then it generates a multiplication factor of the two, so if resolution A is 120% wider and 130% higher than resolution B, the ratio would be 1.2 x 1.3. This way, any control within the form will be resized (along with font sizes) based on this ratio.

Full documentation can be found as standart XML documentation along with the release, as well as in the repository wiki. Note that the wiki is auto-generated using [MarkdownGenerator](https://github.com/neuecc/MarkdownGenerator).

# Features
* Easy and simple to use
* One-time initialization, then easily make forms responsive with a single function, ApplyResponsiveness()
* Additional form features such as auto scrolls / text ellipsis / more.

# Important Note
Do not forget, all magic comes with a price! This library only makes controls larger or smaller based on the screen resolution, it **doesn't changes their positions** as we can not know how you'd like to design your own and custom form. To make a fully responsive form, you should use this library combined with **docking** controls on your form at design-time. You can create beautiful UIs when docking the controls the right way (using containers, etc).
This means that to create a fully-working responsive form you'll need to:
* Initialize the library
* Use the responsive form apply method
* Make sure all controls are docked

# Example
One time initialize
```csharp
      RWF.Initialize(new System.Drawing.Size(1920, 1080)); // This defines the design-time screen resolution. User's screen resolution will be taken automatically.
```
Show form & make it responsive with one method
```csharp
        public static void ShowForm(Form f, bool dialog = false)
        {
            if (f.GetReponsivenessObject() == null)
                RWF.ApplyResponsiveness(f);
            if (dialog)
                f.ShowDialog();
            else
                f.Show();
        }
```
