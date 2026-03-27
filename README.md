# KeySenderApp

A simple **C# Key Sender Tool** for automating key input in **vSphere HTML5 Client** or any other application. Designed for **programmers** and **vSphere users** who need to automate repetitive text or commands.

---

## Features

- Send letters, numbers, Enter, space, and some symbols automatically.
- Custom **delay between keystrokes** (ms) and **start delay** (s).
- **Pause**, **Resume**, and **Stop** functionality.
- Compatible with **vSphere HTML5 Console** (Canvas-based).
- Uses **InputSimulatorPlus** to simulate physical keypresses.
- Supports script comments using `//` at the start of a line.

---

## Installation

### Visual Studio

1. Open the solution in **Visual Studio**.
2. Open **Tools → NuGet Package Manager → Package Manager Console**.
3. Install-Package InputSimulatorPlus

### Vs Code

1. dotnet add package InputSimulatorPlus
