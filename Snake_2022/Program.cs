using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace Snake_2022
{

    class Program
    {
        private const int MF_BYCOMMAND = 0x00000000;
        public const int SC_CLOSE = 0xF060;
        public const int SC_MINIMIZE = 0xF020;
        public const int SC_MAXIMIZE = 0xF030;
        public const int SC_SIZE = 0xF000;

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        static int playzone_w = 57;
        static int playzone_h = 34;
        static int playzone_t = 3;
        static int playzone_l = 2;
        static int rabbits = 0;
        static int score = 0;
        static int speed = 1;
        static int level = 1;
        static double health = 20;
        static int[] record = new int[11];
        static string[] player = new string[11];
        static void Main(string[] args)
        {
            //Запрет на изменение размеров окна
            IntPtr handle = GetConsoleWindow();
            IntPtr sysMenu = GetSystemMenu(handle, false);

            if (handle != IntPtr.Zero)
            {
                DeleteMenu(sysMenu, SC_CLOSE, MF_BYCOMMAND);
                DeleteMenu(sysMenu, SC_MINIMIZE, MF_BYCOMMAND);
                DeleteMenu(sysMenu, SC_MAXIMIZE, MF_BYCOMMAND);
                DeleteMenu(sysMenu, SC_SIZE, MF_BYCOMMAND);
            }
            //параметры окна
            Console.SetWindowSize(120, 40);
            Console.SetBufferSize(120, 40);
            Console.CursorVisible = false;
            while (true)
            {
                //Главное меню
                switch (MainMenu())
                {
                    case 1: level = 1; break;
                    case 2: level = 2; break;
                    case 3: level = 3; break;
                    case 4: return;
                }
                //игра и отработка конца игры
                GameOver(playGame());
            }
        }
        private static int playGame()
        {
            bool resetTable = false;
            if (resetTable)
            {
                Properties.Settings.Default.player1 = "";
                Properties.Settings.Default.player2 = "";
                Properties.Settings.Default.player3 = "";
                Properties.Settings.Default.player4 = "";
                Properties.Settings.Default.player5 = "";
                Properties.Settings.Default.player6 = "";
                Properties.Settings.Default.player7 = "";
                Properties.Settings.Default.player8 = "";
                Properties.Settings.Default.player9 = "";
                Properties.Settings.Default.player10 = "";
                Properties.Settings.Default.record1 = 0;
                Properties.Settings.Default.record2 = 0;
                Properties.Settings.Default.record3 = 0;
                Properties.Settings.Default.record4 = 0;
                Properties.Settings.Default.record5 = 0;
                Properties.Settings.Default.record6 = 0;
                Properties.Settings.Default.record7 = 0;
                Properties.Settings.Default.record8 = 0;
                Properties.Settings.Default.record9 = 0;
                Properties.Settings.Default.record10 = 0;
                Properties.Settings.Default.Save();
            }

            //Параметры головы
            Random rnd = new Random();
            rabbits = 0;
            score = 0;
            speed = 1;
            health = 20;
            int delay = 50;
            int head_x = 20;
            int head_y = 20;
            byte dir = 1; //направление
            int tail_x = head_x;
            int tail_y = head_y;
            byte[,] location = new byte[playzone_w, playzone_h]; //локация
            bool gotit = false; //съел кролика

            //отрисовка игровой зоны
            Console.Clear();
            showLocation();
            showProperties();
            if (level>1)
            {
                spawnStones(location, rnd, head_x, head_y);
            }
            Rabbit rabbit = new Rabbit(head_x, head_y, location, rnd, playzone_w, playzone_h, playzone_l, playzone_t);

            while (true)
            {
                //управление
                if (Console.KeyAvailable == true)
                {
                    Console.SetCursorPosition(0, 0);
                    ConsoleKeyInfo key;
                    key = Console.ReadKey();
                    Console.SetCursorPosition(0, 0);
                    Console.Write(" ");
                    switch (key.Key)
                    {
                        case ConsoleKey.D: if (dir != 2) dir = 1; break;
                        case ConsoleKey.A: if (dir != 1) dir = 2; break;
                        case ConsoleKey.W: if (dir != 4) dir = 3; break;
                        case ConsoleKey.S: if (dir != 3) dir = 4; break;
                        case ConsoleKey.Escape: return 4;
                    }
                }
                location[head_x, head_y] = dir;
                //голова
                switch (dir)
                {
                    case 1: head_x += 1; break;
                    case 2: head_x -= 1; break;
                    case 3: head_y -= 1; break;
                    case 4: head_y += 1; break;
                }
                if (head_x < 0) head_x = playzone_w - 1;
                if (head_x > playzone_w - 1) head_x = 0;
                if (head_y < 0) head_y = playzone_h - 1;
                if (head_y > playzone_h - 1) head_y = 0;

                //коллизии
                switch (collision(location[head_x, head_y]))
                {
                    case 0: break;
                    case 1: return 1;
                    case 2: gotit = true; Console.Beep(); break;
                    case 3: return 2;
                }
                //голод
                health -= 0.04;
                Console.SetCursorPosition(70, 1);
                Console.Write("Сытость:                                ");
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                if (health < 13) Console.ForegroundColor = ConsoleColor.DarkYellow;
                if (health < 7) Console.ForegroundColor = ConsoleColor.DarkRed;
                for (int i = 1; i <= health; i++)
                {
                    Console.SetCursorPosition(80 + i, 1);
                    Console.Write("█");

                }
                if (health < 0) return 3;
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.SetCursorPosition((playzone_l + head_x) * 2, playzone_t + head_y);
                Console.Write("██");
                Console.ForegroundColor = ConsoleColor.White;

                //хвост
                if (!gotit)
                {
                    Console.SetCursorPosition((playzone_l + tail_x) * 2, playzone_t + tail_y);
                    Console.Write("  ");
                    switch (location[tail_x, tail_y])
                    {
                        case 1: location[tail_x, tail_y] = 0; tail_x += 1; break;
                        case 2: location[tail_x, tail_y] = 0; tail_x -= 1; break;
                        case 3: location[tail_x, tail_y] = 0; tail_y -= 1; break;
                        case 4: location[tail_x, tail_y] = 0; tail_y += 1; break;
                    }
                    if (tail_x < 0) tail_x = playzone_w - 1;
                    if (tail_x > playzone_w - 1) tail_x = 0;
                    if (tail_y < 0) tail_y = playzone_h - 1;
                    if (tail_y > playzone_h - 1) tail_y = 0;
                    // перемещение кролика
                    if (level==3)
                    {
                        rabbit.Move(rnd, location, playzone_l, playzone_t, playzone_w, playzone_h, head_x, head_y, tail_x, tail_y);
                    }
                }
                else
                {
                    health += 3;
                    if (health > 20) health = 20;
                    if (level == 1) score += 1;
                    if (level == 2) score += 2;
                    if (level == 3) score += 4;
                    gotit = false;
                    rabbits++;
                    if (rabbits % 10 == 0) speed++;
                    if (speed > 10) speed = 10;
                    switch (speed)
                    {
                        case 1: delay = 50; break;
                        case 2: delay = 43; break;
                        case 3: delay = 36; break;
                        case 4: delay = 30; break;
                        case 5: delay = 24; break;
                        case 6: delay = 19; break;
                        case 7: delay = 14; break;
                        case 8: delay = 10; break;
                        case 9: delay = 7; break;
                        case 10: delay = 5; break;
                          
                    }
                    showProperties();
                    rabbit = new Rabbit(head_x, head_y, location, rnd, playzone_w, playzone_h, playzone_l, playzone_t);
                }

                System.Threading.Thread.Sleep(delay);
            }
        }
        private static bool spawnStones(byte [,] location, Random rnd, int head_x, int head_y)
        {
            int stone_x, stone_y;
            bool f = false;
            for (int i =0; i<10;i++)
            {
                do
                {
                    f = false;
                    stone_x = rnd.Next(playzone_w);
                    stone_y = rnd.Next(playzone_h);
                    if (location[stone_x, stone_y] != 0) f = true;
                    if (stone_x == head_x && stone_y == head_y) f = true;
                } while (f);

                location[stone_x, stone_y] = 6;
                Console.SetCursorPosition((playzone_l + stone_x) * 2, playzone_t + stone_y);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("██");
            }
            return true;
        }
        private static int collision(byte field)
        {
            switch (field)
            {
                case 0: return 0;//препятствий нет
                case 1: return 1;//удав съел сам себя
                case 2: return 1;
                case 3: return 1;
                case 4: return 1;
                case 5: return 2;//удав съел кролика
                case 6: return 3;//удав ударился в камень
            }
            return 0;
        }


        static void showLocation()
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            drawDowbleRect((playzone_l - 1) * 2, playzone_t - 1, playzone_w * 2 + 4, playzone_h + 2);
            Console.ForegroundColor = ConsoleColor.White;
        }
        static void showProperties()
        {
            for (int x = 0; x < 120; x++)
            {
                Console.SetCursorPosition(x, 0);
                Console.Write(" ");
            }
            Console.SetCursorPosition(2, 1);
            Console.Write("Скорость: {0}", speed);
            Console.SetCursorPosition(20, 1);
            Console.Write("Съедено кроликов: {0}",rabbits);
            Console.SetCursorPosition(50, 1);
            Console.Write("Счёт: {0}", score);
        }
        static void drawRect(int left, int top, int width, int height)
        {
            for (int x = left; x < left + width - 1; x++)
            {
                Console.SetCursorPosition(x, top);
                Console.Write("─");
                Console.SetCursorPosition(x, top + height - 1);
                Console.Write("─");
            }
            for (int y = top; y < top + height - 1; y++)
            {
                Console.SetCursorPosition(left, y);
                Console.Write("│");
                Console.SetCursorPosition(left + width - 1, y);
                Console.Write("│");
            }
            Console.SetCursorPosition(left, top); Console.Write("┌");
            Console.SetCursorPosition(left + width - 1, top); Console.Write("┐");
            Console.SetCursorPosition(left, top + height - 1); Console.Write("└");
            Console.SetCursorPosition(left + width - 1, top + height - 1); Console.Write("┘");
        }

        static void drawDowbleRect(int left, int top, int width, int height)
        {
            for (int x = left; x < left + width - 1; x++)
            {
                Console.SetCursorPosition(x, top);
                Console.Write("═");
                Console.SetCursorPosition(x, top + height - 1);
                Console.Write("═");
            }
            for (int y = top; y < top + height - 1; y++)
            {
                Console.SetCursorPosition(left, y);
                Console.Write("║");
                Console.SetCursorPosition(left + width - 1, y);
                Console.Write("║");
            }
            Console.SetCursorPosition(left, top); Console.Write("╔");
            Console.SetCursorPosition(left + width - 1, top); Console.Write("╗");
            Console.SetCursorPosition(left, top + height - 1); Console.Write("╚");
            Console.SetCursorPosition(left + width - 1, top + height - 1); Console.Write("╝");
        }
        static void drawButton(string text,bool selected, int left, int top, int width=0, int height=0)
        {
            int l = text.Length;
            //кнопка под размер текста
            if (width == 0 && height == 0)
            {
                if (selected)
                {
                    drawDowbleRect(left, top, l + 2, 3);
                }
                else
                {
                    drawRect(left, top, l + 2, 3);
                }
                Console.SetCursorPosition(left+1, top+1);
                Console.Write(text);
            } else // фиксированный размер
            {

            }
        }
        private static int MainMenu()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Blue;
            drawDowbleRect(20, 8, 80, 24);

            // -------- Меню --------
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.SetCursorPosition(35, 2);
            Console.Write("       █    ██     █████     █████     █████");
            Console.SetCursorPosition(35, 3);
            Console.Write("       █    ██    █   ██    █   ███    █   ██");
            Console.SetCursorPosition(35, 4);
            Console.Write("        ██████    █   ██    █   ███    █████");
            Console.SetCursorPosition(35, 5);
            Console.Write("            ██    █   ██    ███████    █    ██");
            Console.SetCursorPosition(35, 6);
            Console.Write("        █████    ████████   █   ███    ██████");
            Console.ForegroundColor = ConsoleColor.White;

            Console.SetCursorPosition(35, 10);
            Console.Write("МЕНЮ");
            Console.ForegroundColor = ConsoleColor.Green;
            Button level1 = new Button("          Уровень 1          ", 22, 12, true); level1.Draw();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Button level2 = new Button("          Уровень 2          ", 22, 15, false); level2.Draw();
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Button level3 = new Button("          Уровень 3          ", 22, 18, false); level3.Draw();
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Button exit = new Button("            ВЫХОД            ", 22, 21, false); exit.Draw();
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(22, 25);
            Console.Write("1. Переключение меню стрелками");
            Console.SetCursorPosition(22, 26);
            Console.Write("2. Выбор - 'Enter'");
            Console.SetCursorPosition(22, 27);
            Console.Write("3. Управление змейкой A,D,W,S");
            Console.SetCursorPosition(22, 28);
            Console.Write("4. Выход из игры в меню - 'Esc'");
            Console.SetCursorPosition(70, 10);
            Console.Write("ТАБЛИЦА РЕКОРДОВ");

            getLeaderboard();

            for (int i = 1; i < 11; i++)
            {
                string s = new String('.', 32 - player[i].Length);
                s = player[i] + s;
                Console.SetCursorPosition(60, 11+i);
                Console.Write("{0}{1,5}",s, record[i]);
            }

            int selectedButton = 1;
            while (true)
            {
                ConsoleKeyInfo key;
                Console.SetCursorPosition(0,0);
                key = Console.ReadKey();
                Console.Write(" ");
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow: if (selectedButton > 1) selectedButton --; break;
                    case ConsoleKey.DownArrow: if (selectedButton < 4) selectedButton ++; break;
                    case ConsoleKey.Enter:
                        switch (selectedButton)
                        {
                            case 1: return 1;
                            case 2: return 2;
                            case 3: return 3;
                            case 4: return 4;
                        }
                        break;
                }
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                level1.Selected = false; level1.Draw();
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                level2.Selected = false; level2.Draw();
                Console.ForegroundColor = ConsoleColor.DarkRed;
                level3.Selected = false; level3.Draw();
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                exit.Selected = false; exit.Draw();
                Console.ForegroundColor = ConsoleColor.White;
                switch (selectedButton)
                {
                    case 1: level1.Selected=true; Console.ForegroundColor = ConsoleColor.Green; level1.Draw(); break;
                    case 2: level2.Selected = true; Console.ForegroundColor = ConsoleColor.Yellow; level2.Draw(); break;
                    case 3: level3.Selected = true; Console.ForegroundColor = ConsoleColor.Red; level3.Draw(); break;
                    case 4: exit.Selected = true; Console.ForegroundColor = ConsoleColor.Magenta; exit.Draw(); break;
                }
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        private static bool GameOver(int deathreason)
        {
            Console.ForegroundColor = ConsoleColor.White;
            int i = 17;
            string cl = new string(' ', 78);
            drawRect(20, 15, 80, 10);
            for (int j=16; j<24; j++)
            {
                Console.SetCursorPosition(21, j);
                Console.Write(cl);
            }

            Console.SetCursorPosition(21, i); i++;
            switch (deathreason)
            {
                case 1: //проиграл
                    Console.Write("                              УДАВ СЪЕЛ САМ СЕБЯ");
                    break;
                case 2: //проиграл
                    Console.Write("                            УДАВ СЛОМАЛ ШЕЮ О КАМЕНЬ");
                    break;
                case 3: //проиграл
                    Console.Write("                              УДАВ УМЕР С ГОЛОДУ");
                    break;
                case 4: //выход
                    Console.Write("                               Вы закончили игру");
                    break;

            }
            int place = getLeaderboardPosition(score);

            if (place < 11)
            {
                i++;
                Console.SetCursorPosition(21, i);i++; i++;
                Console.Write("             ПОЗДРАВЛЯЕМ!!! Вы попали в таблицу лидеров на {0} место", place);
                Console.SetCursorPosition(21, i); i++;
                Console.Write("             Введите своё имя:   ", place);
                Console.CursorVisible = true;
                string name = Console.ReadLine();
                if (name.Length > 30) name = name.Substring(1, 30);
                Console.CursorVisible = false;

                for (int j = 10; j > place; j--)
                {
                    record[j] = record[j - 1];
                    player[j] = player[j - 1];
                }
                record[place] = score;
                player[place] = name;
                setLeaderboard();
            } else
            {
                i++;
                Console.SetCursorPosition(21, i); i++; i++;
                Console.Write("             Для возврата в главное меню нажмите любую клавишу");
                Console.ReadKey();
            }
            return true;
        }
        private static int getLeaderboardPosition(int score)
        {
            getLeaderboard();
            for (int i=1; i<10; i++)
            {
                if (score > record[i]) return i;
            }
            return 11;
        }
        private static bool getLeaderboard()
        {
            // -------- Таблица рекордов --------
            player[1] = Properties.Settings.Default.player1;
            player[2] = Properties.Settings.Default.player2;
            player[3] = Properties.Settings.Default.player3;
            player[4] = Properties.Settings.Default.player4;
            player[5] = Properties.Settings.Default.player5;
            player[6] = Properties.Settings.Default.player6;
            player[7] = Properties.Settings.Default.player7;
            player[8] = Properties.Settings.Default.player8;
            player[9] = Properties.Settings.Default.player9;
            player[10] = Properties.Settings.Default.player10;
            record[1] = Properties.Settings.Default.record1;
            record[2] = Properties.Settings.Default.record2;
            record[3] = Properties.Settings.Default.record3;
            record[4] = Properties.Settings.Default.record4;
            record[5] = Properties.Settings.Default.record5;
            record[6] = Properties.Settings.Default.record6;
            record[7] = Properties.Settings.Default.record7;
            record[8] = Properties.Settings.Default.record8;
            record[9] = Properties.Settings.Default.record9;
            record[10] = Properties.Settings.Default.record10;
            return true;
        }
        private static bool setLeaderboard()
        {
            // -------- Таблица рекордов --------
            Properties.Settings.Default.player1 = player[1];
            Properties.Settings.Default.player2 = player[2];
            Properties.Settings.Default.player3 = player[3];
            Properties.Settings.Default.player4 = player[4];
            Properties.Settings.Default.player5 = player[5];
            Properties.Settings.Default.player6 = player[6];
            Properties.Settings.Default.player7 = player[7];
            Properties.Settings.Default.player8 = player[8];
            Properties.Settings.Default.player9 = player[9];
            Properties.Settings.Default.player10 = player[10];
            Properties.Settings.Default.record1 = record[1];
            Properties.Settings.Default.record2 = record[2];
            Properties.Settings.Default.record3 = record[3];
            Properties.Settings.Default.record4 = record[4];
            Properties.Settings.Default.record5 = record[5];
            Properties.Settings.Default.record6 = record[6];
            Properties.Settings.Default.record7 = record[7];
            Properties.Settings.Default.record8 = record[8];
            Properties.Settings.Default.record9 = record[9];
            Properties.Settings.Default.record10 = record[10];
            Properties.Settings.Default.Save();
            return true;
        }
    }
    public class Rabbit
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Speed { get; set; } //скорость перемещения кролика
        public double Direction_x { get; set; } //коэффициент перемещения по X
        public double Direction_y { get; set; } //коэффициент перемещения по y
        public Rabbit(int head_x, int head_y, byte[,] location, Random rnd, int playzone_w, int playzone_h, int playzone_l, int playzone_t)
        {
               int rabbit_x, rabbit_y;
                bool f = false;
                do
                {
                    f = false;
                    rabbit_x = rnd.Next(playzone_w);
                    rabbit_y = rnd.Next(playzone_h);
                    if (location[rabbit_x, rabbit_y] != 0) f = true;
                    if (rabbit_x == head_x && rabbit_y == head_y) f = true;
                } while (f);

                location[rabbit_x, rabbit_y] = 5;
            X = rabbit_x;
            Y = rabbit_y;
            Speed = 0.25;
            Direction_x = (rnd.NextDouble() * 2 - 1) * Speed; //выбираем направление движения
            Direction_y = (rnd.NextDouble() * 2 - 1) * Speed;
            Console.SetCursorPosition((playzone_l + rabbit_x) * 2, playzone_t + rabbit_y);
            Console.Write("$");
        }
        public bool Move(Random rnd, byte[,] location, int playzone_l, int playzone_t, int playzone_w, int playzone_h, int head_x, int head_y, int tail_x, int tail_y)
        {

            double next_x = X + Direction_x;
            double next_y = Y + Direction_y;
            int x = Convert.ToInt32(X);
            int y = Convert.ToInt32(Y);
            location[x, y] = 0;
            if (next_x > playzone_w - 1 || next_x < 0)
            {
                Direction_x = -Direction_x;
                next_x = X + Direction_x * 2;
            }
            if (next_y > playzone_h - 1 || next_y < 0)
            {
                Direction_y = -Direction_y;
                next_y = Y + Direction_y * 2;
            }

            if (location[Convert.ToInt32(next_x), Convert.ToInt32(next_y)] != 0)
            {
                Direction_x = (rnd.NextDouble() * 2 - 1) * Speed; //выбираем направление движения
                Direction_y = (rnd.NextDouble() * 2 - 1) * Speed;
                return false;
            }

            if (head_x == Convert.ToInt32(next_x) && head_y == Convert.ToInt32(next_y))
            {
                Direction_x = (rnd.NextDouble() * 2 - 1) * Speed; //выбираем направление движения
                Direction_y = (rnd.NextDouble() * 2 - 1) * Speed;
                return false;
            }

            if (tail_x == Convert.ToInt32(next_x) && tail_y == Convert.ToInt32(next_y))
            {
                Direction_x = (rnd.NextDouble() * 2 - 1) * Speed; //выбираем направление движения
                Direction_y = (rnd.NextDouble() * 2 - 1) * Speed;
                return false;
            }
            Console.SetCursorPosition((playzone_l + x) * 2, playzone_t + y);
            Console.Write(" ");

            X = next_x; Y = next_y;
            location[Convert.ToInt32(X), Convert.ToInt32(Y)] = 5;
            Console.SetCursorPosition((playzone_l + Convert.ToInt32(X)) * 2, playzone_t + Convert.ToInt32(Y));
            Console.Write("$");
            return true;
        }
    }
    public class Button
    {
        public string Text { get; set; }
        public bool Selected { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Button(string text, int left, int top, bool selected=false, int width=0, int height=0)
        {
            Text = text;
            Selected = selected;
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }
        public bool Draw()
        {
            int l = Text.Length;
            if (Selected)
            {
                drawDowbleRect(Left, Top, l + 2, 3);
            }
            else
            {
                drawRect(Left, Top, l + 2, 3);
            }
            Console.SetCursorPosition(Left + 1, Top + 1);
            Console.Write(Text);
            return true;
        }
        private bool select()
        {
            drawDowbleRect(Left, Top, Text.Length + 2, 3);
            return true;
        }
        private bool deselect()
        {
            drawRect(Left, Top, Text.Length + 2, 3);
            return true;
        }
        private bool drawDowbleRect(int left, int top, int width, int height)
        {
            for (int x = left; x < left + width - 1; x++)
            {
                Console.SetCursorPosition(x, top);
                Console.Write("═");
                Console.SetCursorPosition(x, top + height - 1);
                Console.Write("═");
            }
            for (int y = top; y < top + height - 1; y++)
            {
                Console.SetCursorPosition(left, y);
                Console.Write("║");
                Console.SetCursorPosition(left + width - 1, y);
                Console.Write("║");
            }
            Console.SetCursorPosition(left, top); Console.Write("╔");
            Console.SetCursorPosition(left + width - 1, top); Console.Write("╗");
            Console.SetCursorPosition(left, top + height - 1); Console.Write("╚");
            Console.SetCursorPosition(left + width - 1, top + height - 1); Console.Write("╝");
            return true;
        }
        private bool drawRect(int left, int top, int width, int height)
        {
            for (int x = left; x < left + width - 1; x++)
            {
                Console.SetCursorPosition(x, top);
                Console.Write("─");
                Console.SetCursorPosition(x, top + height - 1);
                Console.Write("─");
            }
            for (int y = top; y < top + height - 1; y++)
            {
                Console.SetCursorPosition(left, y);
                Console.Write("│");
                Console.SetCursorPosition(left + width - 1, y);
                Console.Write("│");
            }
            Console.SetCursorPosition(left, top); Console.Write("┌");
            Console.SetCursorPosition(left + width - 1, top); Console.Write("┐");
            Console.SetCursorPosition(left, top + height - 1); Console.Write("└");
            Console.SetCursorPosition(left + width - 1, top + height - 1); Console.Write("┘");
            return true;
        }
    }
}
