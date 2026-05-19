using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace KeySenderApp
{
    public partial class Form1 : Form
    {
        TextBox inputBox;
        Button sendButton, pauseButton, stopButton;
        NumericUpDown delayInput, startDelayInput;
        Label statusLabel;

        bool isPaused = false;
        bool isRunning = false;
        bool isStopped = false;

        InputSimulator sim = new InputSimulator(); // InputSimulatorPlus instance

        public Form1()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Key Sender Tool";
            this.Width = 600;
            this.Height = 400;
            this.TopMost = true;

            inputBox = new TextBox
            {
                Multiline = true,
                Width = 550,
                Height = 250,
                Top = 10,
                Left = 10
            };

            sendButton = new Button
            {
                Text = "Send",
                Top = 300,
                Left = 10
            };
            sendButton.Click += SendButton_Click;

            delayInput = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 1000,
                Value = 50,
                Top = 300,
                Left = 100
            };
            Label delayLabel = new Label
            {
                Text = "Delay (ms)",
                Top = 280,
                Left = 100
            };

            startDelayInput = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 30,
                Value = 3,
                Top = 300,
                Left = 200
            };
            Label startLabel = new Label
            {
                Text = "Start Delay (s)",
                Top = 280,
                Left = 200
            };

            pauseButton = new Button
            {
                Text = "Pause",
                Top = 300,
                Left = 350
            };
            pauseButton.Click += PauseButton_Click;

            stopButton = new Button
            {
                Text = "Stop",
                Top = 300,
                Left = 430
            };
            stopButton.Click += StopButton_Click;

            statusLabel = new Label
            {
                Text = "Ready",
                Top = 330,
                Left = 10,
                Width = 500
            };

            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;

            this.Controls.AddRange(new Control[]
            {
                inputBox, sendButton, delayInput, delayLabel,
                startDelayInput, startLabel,
                pauseButton, stopButton,
                statusLabel
            });
        }

        private void PauseButton_Click(object sender, EventArgs e)
        {
            isPaused = !isPaused;
            pauseButton.Text = isPaused ? "Resume" : "Pause";
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            isStopped = true;
            isRunning = false;
            statusLabel.Text = "Stopped ❌";
        }

        private async void SendButton_Click(object sender, EventArgs e)
        {
            if (isRunning) return;

            isRunning = true;
            isStopped = false;

            int startDelay = (int)startDelayInput.Value;
            int delay = (int)delayInput.Value;

            var lines = ParseScript(inputBox.Text);

            statusLabel.Text = $"Counting down {startDelay} seconds...";
            await Task.Delay(startDelay * 1000);

            statusLabel.Text = "Running...";

            foreach (var line in lines)
            {
                if (isStopped) break;

                await SendLine(line, delay);
            }

            if (!isStopped)
                statusLabel.Text = "Done ✅";

            isRunning = false;
        }

        private List<string> ParseScript(string text)
        {
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var result = new List<string>();

            foreach (var line in lines)
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith("//")) continue; // Ignore comments
                result.Add(line);
            }

            return result;
        }

        private async Task SendLine(string line, int delay)
        {
            var words = line.Split(' ');
            foreach (var word in words)
            {
                foreach (char c in word)
                {
                    while (isPaused)
                        await Task.Delay(100);

                    SendChar(c);
                    await Task.Delay(delay);
                }

                // Send a space after each word
                sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);
                await Task.Delay(delay);

                if (isStopped) return;
            }

            // Press Enter at the end of each line
            sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            await Task.Delay(delay * 2);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.OemQuestion) // Ctrl + /
            {
                int start = inputBox.SelectionStart;
                int lineIndex = inputBox.GetLineFromCharIndex(start);
                int lineStart = inputBox.GetFirstCharIndexFromLine(lineIndex);
                inputBox.Text = inputBox.Text.Insert(lineStart, "//");
            }
        }

        private void SendChar(char c)
        {
            if (c >= 'a' && c <= 'z')
            {
                sim.Keyboard.KeyPress((VirtualKeyCode)((int)VirtualKeyCode.VK_A + (c - 'a')));
            }
            else if (c >= 'A' && c <= 'Z')
            {
                sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                sim.Keyboard.KeyPress((VirtualKeyCode)((int)VirtualKeyCode.VK_A + (c - 'A')));
                sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
            }
            else if (c >= '0' && c <= '9')
            {
                sim.Keyboard.KeyPress((VirtualKeyCode)((int)VirtualKeyCode.VK_0 + (c - '0')));
            }
            else
            {
                switch (c)
                {
                    case '.': sim.Keyboard.KeyPress(VirtualKeyCode.OEM_PERIOD); break;
                    case ',': sim.Keyboard.KeyPress(VirtualKeyCode.OEM_COMMA); break;
                    case '-': sim.Keyboard.KeyPress(VirtualKeyCode.OEM_MINUS); break;

                    case '_':
                        sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                        sim.Keyboard.KeyPress(VirtualKeyCode.OEM_MINUS);
                        sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                        break;

                    case '@':
                        sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                        sim.Keyboard.KeyPress(VirtualKeyCode.VK_2);
                        sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                        break;

                    // ✅ ADD THESE ↓↓↓

                    case '"':
                        sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                        sim.Keyboard.KeyPress(VirtualKeyCode.OEM_7); // "
                        sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                        break;

                    case '=':
                        sim.Keyboard.KeyPress(VirtualKeyCode.OEM_PLUS);
                        break;

                    case ':':
                        sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                        sim.Keyboard.KeyPress(VirtualKeyCode.OEM_1);
                        sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                        break;

                    case '/':
                        sim.Keyboard.KeyPress(VirtualKeyCode.OEM_2);
                        break;

                    case '\\':
                        sim.Keyboard.KeyPress(VirtualKeyCode.OEM_5);
                        break;

                    case '?':
                        sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                        sim.Keyboard.KeyPress(VirtualKeyCode.OEM_2);
                        sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                        break;

                    case '&':
                        sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                        sim.Keyboard.KeyPress(VirtualKeyCode.VK_7);
                        sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                        break;

                    case '%':
                        sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                        sim.Keyboard.KeyPress(VirtualKeyCode.VK_5);
                        sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                        break;

                    case '#':
                        sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                        sim.Keyboard.KeyPress(VirtualKeyCode.VK_3);
                        sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                        break;

                    case '+':
                        sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                        sim.Keyboard.KeyPress(VirtualKeyCode.OEM_PLUS);
                        sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                        break;
                    case ' ':
                        sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);
                        break;
                    case '<':
                        sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                        sim.Keyboard.KeyPress(VirtualKeyCode.OEM_COMMA); // Shift + , = <
                        sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                        break;
                    case '>':
                        sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                        sim.Keyboard.KeyPress(VirtualKeyCode.OEM_PERIOD); // Shift + . = >
                        sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                        break;
                    case '|':
                        sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                        sim.Keyboard.KeyPress(VirtualKeyCode.OEM_5); // Shift + \ = |
                        sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                        break;
                    case '!':
                        sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                        sim.Keyboard.KeyPress(VirtualKeyCode.VK_1);
                        sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                        break;
                    case '(':
                        sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                        sim.Keyboard.KeyPress(VirtualKeyCode.VK_9);
                        sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                        break;
                    case ')':
                        sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                        sim.Keyboard.KeyPress(VirtualKeyCode.VK_0);
                        sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                        break;
                    case '{':
                        sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                        sim.Keyboard.KeyPress(VirtualKeyCode.OEM_4); // [
                        sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                        break;
                    case '}':
                        sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                        sim.Keyboard.KeyPress(VirtualKeyCode.OEM_6); // ]
                        sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                        break;
                    case '[':
                        sim.Keyboard.KeyPress(VirtualKeyCode.OEM_4);
                        break;
                    case ']':
                        sim.Keyboard.KeyPress(VirtualKeyCode.OEM_6);
                        break;
                    case ';':
                        sim.Keyboard.KeyPress(VirtualKeyCode.OEM_1);
                        break;
                    case '\'':
                        sim.Keyboard.KeyPress(VirtualKeyCode.OEM_7);
                        break;
                    case '~':
                        sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                        sim.Keyboard.KeyPress(VirtualKeyCode.OEM_3);
                        sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                        break;
                    case '`':
                        sim.Keyboard.KeyPress(VirtualKeyCode.OEM_3);
                        break;
                    case '*':
                        sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                        sim.Keyboard.KeyPress(VirtualKeyCode.VK_8);
                        sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                        break;
                    case '^':
                        sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                        sim.Keyboard.KeyPress(VirtualKeyCode.VK_6);
                        sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                        break;
                    case '$':
                        sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                        sim.Keyboard.KeyPress(VirtualKeyCode.VK_4);
                        sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                        break;
                }
            }
        }
    }
}