# ImForms
ImForms is an experiment in creating a wrapper class that allows you to create and get input from Windows Forms control, but in an fashion similar to Immediate Mode GUI systems such as [Dear ImGui](https://github.com/ocornut/imgui).

[![https://gyazo.com/c7c63b0968575e34d28b0431af08f92e](https://i.gyazo.com/c7c63b0968575e34d28b0431af08f92e.gif)](https://gyazo.com/c7c63b0968575e34d28b0431af08f92e)

ImForms could someday become a minimal-dependency tool that can easily be integrated into an existing Windows Forms project, and allows developers to create GUIs with minimal work.

The class is in ImForms.cs. It's simple to use. Just follow the example project (ImFormsUser.\*.cs). The biggest gotcha is that you get the best performance if you assign every control a unique string identifier. The function `CompileTime.ID()` makes this easy. The second biggest gotcha is that there just aren't very many features at the moment.

This project is just an experiment. If you want to see it become more fully-featured, let tweet at Ozzy know on [Twitter](https://twitter.com/OswaldHurlem).