using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MathMaze
{
    public partial class MathMazeForm : Form
    {
        public MathMazeForm()
        {
            InitializeComponent();
            SetDifficulty(Difficulty.Beginner);
        }

        private void StartGame(bool newMaze)
        {
            if (newMaze)
                GenerateMaze();

            m_current_x = left + cellSize / 2;
            m_current_y = top + cellSize / 2;
            m_direction = Direction.Still;

            Refresh();
            UpdateScene();

            m_exit = false;
            RunGameLoop();
        }

        private void RunGameLoop()
        {
            while (true)
            {
                if (m_exit)
                    break;

                int tick = Environment.TickCount;

                if (m_pause)
                {
                    m_tick = tick;
                }
                else if (tick - m_tick > 16)
                {
                    UpdateGame(tick - m_tick);
                    m_tick = tick;
                }
                Application.DoEvents();
            }
        }

        private void PauseGame()
        {
            m_pause = true;
        }

        private void ResumeGame()
        {
            m_pause = false;
        }

        private void EndGame()
        {
            m_exit = true;
        }

        private void UpdateGame(int timeDiff)
        {
            if (m_direction == Direction.Still)
            {
                return;
            }

            int cellOld_x = ((int)m_current_x - left) / cellSize;
            int cellOld_y = ((int)m_current_y - top) / cellSize;

            if (m_direction == Direction.Left)
                m_current_x -= timeDiff * m_speed;
            if (m_direction == Direction.Right)
                m_current_x += timeDiff * m_speed;
            if (m_direction == Direction.Up)
                m_current_y -= timeDiff * m_speed;
            if (m_direction == Direction.Down)
                m_current_y += timeDiff * m_speed;

            int cell_x = ((int)m_current_x - left) / cellSize;
            int cell_y = ((int)m_current_y - top) / cellSize;

            // out of border?
            if (m_current_x < left || cell_x >= mazeSize || m_current_y < top || cell_y >= mazeSize)
            {
                Fail();
                return;
            }

            int[] affected_x = new int[9] { cell_x, cell_x, cell_x, cell_x + 1, cell_x + 1, cell_x + 1, cell_x - 1, cell_x - 1, cell_x - 1 };
            int[] affected_y = new int[9] { cell_y, cell_y + 1, cell_y - 1, cell_y, cell_y + 1, cell_y - 1, cell_y, cell_y + 1, cell_y - 1 };
            for (int i = 0; i < 9; i++)
            {
                if (affected_x[i] >= 0 && affected_x[i] < mazeSize && affected_y[i] >= 0 && affected_y[i] < mazeSize)
                {
                    UpdateScene(affected_x[i], affected_y[i]);
                }
            }

            // if cell is not changed, we don't need to check anything here
            if (cell_x == cellOld_x && cell_y == cellOld_y)
                return;

            // get to the target?
            if (cell_x == mazeSize - 1 && cell_y == mazeSize - 1)
                Success();

            // rule check
            if (m_number[cell_x, cell_y] % 2 == m_number[cellOld_x, cellOld_y] % 2)
                Fail();
        }

        private void UpdateScene(int cell_x = -1, int cell_y = -1)
        {
            Graphics g = this.CreateGraphics();
            DrawMaze(g, cell_x, cell_y);
            DrawNumbers(g, cell_x, cell_y);
            DrawBall(g);
        }

        private void Success()
        {
            EndGame();
            MessageBox.Show("Congratulations!");
            StartGame(true);
        }

        private void Fail()
        {
            EndGame();
            MessageBox.Show("Failed...");
            StartGame(false);
        }

        private void GenerateMaze()
        {
            do
            {
                GenerateValues();
                System.Diagnostics.Debug.Print("Generate maze...");
            } while (!ValidMaze());
            System.Diagnostics.Debug.Print("Generate maze successfully!");
        }

        private void GenerateValues()
        {
            Random r = new Random();
            for (int i = 0; i < mazeSize; i++)
            {
                for (int j = 0; j < mazeSize; j++)
                {
                    m_number[i, j] = r.Next(1, 99);
                }
            }
        }

        private bool ValidMaze()
        {
            MazeValidator validator = new MazeValidator(mazeSize, m_number);
            return validator.ValidateMaze();
        }

        private void DrawMaze(Graphics g, int cell_x = -1, int cell_y = -1)
        {
            //Pen pen = new Pen(Color.Black);
            //e.Graphics.DrawLine(pen, new Point(29, 29), new Point(30+mazeSize*cellSize+1, 30+mazeSize*cellSize+1));

            Brush brush1 = new SolidBrush(Color.White);
            Brush brush2 = new SolidBrush(Color.FromArgb(220, 220, 220));
            Brush brushStart = new SolidBrush(Color.LightGreen);
            Brush brushEnd = new SolidBrush(Color.LightPink);

            if (cell_x == -1 && cell_y == -1)
            {
                for (int i = 0; i < mazeSize; i++)
                {
                    for (int j = 0; j < mazeSize; j++)
                    {
                        Brush brush = (i + j) % 2 == 0 ? brush1 : brush2;
                        if (i == 0 && j == 0) brush = brushStart;
                        if (i == mazeSize - 1 && j == mazeSize - 1) brush = brushEnd;
                        g.FillRectangle(brush, left + cellSize * i, top + cellSize * j, cellSize, cellSize);
                    }
                }
            }
            else
            {
                int i = cell_x;
                int j = cell_y;
                Brush brush = (i + j) % 2 == 0 ? brush1 : brush2;
                if (i == 0 && j == 0) brush = brushStart;
                if (i == mazeSize - 1 && j == mazeSize - 1) brush = brushEnd;
                g.FillRectangle(brush, left + cellSize * i, top + cellSize * j, cellSize, cellSize);
            }
        }

        private void DrawNumbers(Graphics g, int cell_x = -1, int cell_y = -1)
        {
            if (cell_x == -1 && cell_y == -1)
            {
                for (int i = 0; i < mazeSize; i++)
                {
                    for (int j = 0; j < mazeSize; j++)
                    {
                        RectangleF rect = new RectangleF(left + cellSize * i, top + cellSize * j, cellSize, cellSize);
                        Font font = new Font(FontFamily.GenericSerif, 16);
                        g.DrawString(m_number[i, j].ToString(), font, SystemBrushes.WindowText, rect);
                    }
                }
            }
            else
            {
                int i = cell_x;
                int j = cell_y;
                RectangleF rect = new RectangleF(left + cellSize * i, top + cellSize * j, cellSize, cellSize);
                Font font = new Font(FontFamily.GenericSerif, 16);
                g.DrawString(m_number[i, j].ToString(), font, SystemBrushes.WindowText, rect);
            }
        }

        private void DrawBall(Graphics g)
        {
            Brush brush = new SolidBrush(Color.Red);
            g.FillEllipse(brush, (int)m_current_x-ballSize/2, (int)m_current_y-ballSize/2, ballSize, ballSize);
        }

        private void MathMazeForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (m_pause && e.KeyCode != Keys.Space)
                return;

            switch (e.KeyCode)
            {
                case Keys.Left:
                    m_direction = Direction.Left;
                    break;
                case Keys.Right:
                    m_direction = Direction.Right;
                    break;
                case Keys.Up:
                    m_direction = Direction.Up;
                    break;
                case Keys.Down:
                    m_direction = Direction.Down;
                    break;
                case Keys.Space:
                    if (m_pause)
                        ResumeGame();
                    else
                        PauseGame();
                    break;
            }
        }

        private void MathMazeForm_Shown(object sender, EventArgs e)
        {
            StartGame(true);
        }

        private void MathMazeForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            EndGame();
        }

        private void SetDifficulty(Difficulty difficulty)
        {
            m_difficulty = difficulty;
            m_speed = speedList[(int)difficulty];
        }

        private void UncheckAllMenuItems()
        {
            beginnerToolStripMenuItem.Checked = false;
            easyToolStripMenuItem.Checked = false;
            normalToolStripMenuItem.Checked = false;
            mediumToolStripMenuItem.Checked = false;
            hardToolStripMenuItem.Checked = false;
            impossibleToolStripMenuItem.Checked = false;
        }

        private void beginnerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetDifficulty(Difficulty.Beginner);
            if (!beginnerToolStripMenuItem.Checked)
            {
                UncheckAllMenuItems();
                beginnerToolStripMenuItem.Checked = true;
                EndGame();
                StartGame(true);
            }
        }

        private void easyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetDifficulty(Difficulty.Easy);
            if (!easyToolStripMenuItem.Checked)
            {
                UncheckAllMenuItems();
                easyToolStripMenuItem.Checked = true;
                EndGame();
                StartGame(true);
            }
        }

        private void normalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetDifficulty(Difficulty.Normal);
            if (!normalToolStripMenuItem.Checked)
            {
                UncheckAllMenuItems();
                normalToolStripMenuItem.Checked = true;
                EndGame();
                StartGame(true);
            }
        }

        private void mediumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetDifficulty(Difficulty.Medium);
            if (!mediumToolStripMenuItem.Checked)
            {
                UncheckAllMenuItems();
                mediumToolStripMenuItem.Checked = true;
                EndGame();
                StartGame(true);
            }
        }

        private void hardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetDifficulty(Difficulty.Hard);
            if (!hardToolStripMenuItem.Checked)
            {
                UncheckAllMenuItems();
                hardToolStripMenuItem.Checked = true;
                EndGame();
                StartGame(true);
            }
        }

        private void impossibleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetDifficulty(Difficulty.Impossible);
            if (!impossibleToolStripMenuItem.Checked)
            {
                UncheckAllMenuItems();
                impossibleToolStripMenuItem.Checked = true;
                EndGame();
                StartGame(true);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private const int mazeSize = 10;
        private const int cellSize = 64;
        private const int ballSize = 16;
        private const int top = 50;
        private const int left = 40;

        private int[,] m_number = new int[mazeSize, mazeSize];
        private double m_current_x;
        private double m_current_y;
        private Direction m_direction;
        private double m_speed;
        private bool m_pause = false;
        private bool m_exit = false;
        private int m_tick;
        private Difficulty m_difficulty;

        private enum Direction
        {
            Still,
            Left,
            Right,
            Up,
            Down
        }

        private enum Difficulty
        {
            Beginner,
            Easy,
            Normal,
            Medium,
            Hard,
            Impossible
        }

        private double[] speedList = new double[] { 0.02, 0.035, 0.05, 0.07, 0.1, 0.15 };


    }
}
