using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathMaze
{
    class MazeValidator
    {
        public MazeValidator(int mazeSize, int[,] mazeData)
        {
            m_mazeSize = mazeSize;

            m_mazeData = new int[mazeSize, mazeSize];
            for (int i = 0; i < mazeSize; i++)
                for (int j = 0; j < mazeSize; j++)
                    m_mazeData[i, j] = mazeData[i, j];

            m_mark = new bool[mazeSize, mazeSize];
            for (int i = 0; i < mazeSize; i++)
                for (int j = 0; j < mazeSize; j++)
                    m_mark[i, j] = false;
        }

        public bool ValidateMaze()
        {
            TryNode(0, 0, 0, 0, false);

            int x = 0;
            int y = 0;
            while (m_queue_x.Count > 0)
            {
                x = m_queue_x.Dequeue();
                y = m_queue_y.Dequeue();
                if (TryNode(x, y, x - 1, y)) return true;
                if (TryNode(x, y, x, y - 1)) return true;
                if (TryNode(x, y, x + 1, y)) return true;
                if (TryNode(x, y, x, y + 1)) return true;
            }

            return false;
        }

        private bool TryNode(int oldx, int oldy, int x, int y, bool checkRule = true)  // return true means find the target, false means not yet
        {
            if (x < 0 || y < 0 || x > m_mazeSize - 1 || y > m_mazeSize - 1)
                return false;

            // rule
            if (checkRule)
            {
                if (m_mazeData[oldx, oldy] % 2 == m_mazeData[x, y] % 2)
                    return false;
            }

            if (x == m_mazeSize - 1 && y == m_mazeSize - 1)
                return true;

            if (m_mark[x, y])
                return false;

            m_queue_x.Enqueue(x);
            m_queue_y.Enqueue(y);
            m_mark[x, y] = true;

            return false;
        }

        private int m_mazeSize;
        private int[,] m_mazeData;

        private Queue<int> m_queue_x = new Queue<int>();
        private Queue<int> m_queue_y = new Queue<int>();
        private bool[,] m_mark;
    }
}
