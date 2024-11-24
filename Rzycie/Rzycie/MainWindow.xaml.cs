using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Rzycie
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        bool _isRunning = false;
        bool[][] newState = new bool[2][];
        Button[][] fields;
        Ellipse[][] ellipses;
        int _died = 0;
        int _birth = 0;
        int _turn = 0;
        bool _zoom = false;
        bool _isCircle = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        
        private void animationProcess()
        {
            while(_isRunning)
            {
                Dispatcher.Invoke(new Action(step));
                int V = (int)Dispatcher.Invoke(() => velocity.Value);
                Thread.Sleep((1000/V));
            }
        }


        private void animation(object sender, RoutedEventArgs e)
        {
            var animation = sender as Button;
            if(animation.Background != Brushes.Green) animation.Background = Brushes.Green;
            else animation.Background = Brushes.Red;
            _isRunning = !_isRunning;

            if (_isRunning) Task.Factory.StartNew(new Action(animationProcess));
        }

        private void add(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (!_isCircle)
            {
                if (button.Background == Brushes.White)
                {
                    button.Background = Brushes.Black;
                }
                else
                {
                    button.Background = Brushes.White;
                }
            }
            else
            {
                string[] cords = button.Name.Split('A');
                var x = int.Parse(cords[1]);
                var y = int.Parse(cords[2]);
                if (canvas.Children.Contains(ellipses[x][y]))
                {
                    canvas.Children.Remove(ellipses[x][y]);
                }
                else
                {
                    canvas.Children.Add(ellipses[x][y]);
                }
            }
        }

        private void zoom(object sender, RoutedEventArgs e)
        {
            save("tmp");
            var button = sender as Button;

            if (!_zoom)
            {
                SV.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                SV.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                canvas.Height = SV.ActualHeight * 5;
                canvas.Width = SV.ActualWidth * 5;
            }
            else
            {
                SV.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                SV.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                canvas.Height = SV.ActualHeight;
                canvas.Width = SV.ActualWidth;
            }
            read("tmp");
            _zoom = !_zoom;
            File.Delete("tmp.gol");
            if(_zoom)
            {
                if (button.Name != "")
                {
                    Dispatcher.BeginInvoke(() => button.BringIntoView());
                }
            }
        }

        private void create(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
            int iSizeY = int.Parse(sizeY.Text);
            int iSizeX = int.Parse(sizeY.Text);

            fields = new Button[iSizeY][];
            ellipses = new Ellipse[iSizeY][];

            for (int i = 0; i < iSizeY; i++)
            {
                fields[i] = new Button[iSizeX];
                ellipses[i] = new Ellipse[iSizeX];
            }

            newState[0] = new bool[iSizeX];
            newState[1] = new bool[iSizeX];

            double x = canvas.ActualWidth / iSizeX;
            double y = canvas.ActualHeight / iSizeY;

            for (int i = 0; i < iSizeY; i++)
            {
                for(int j = 0; j < iSizeX; j++)
                {
                    fields[i][j] = new Button();
                    fields[i][j].Name = "Button" + "A" + i.ToString() + "A" + j.ToString();
                    fields[i][j].Background = Brushes.White;
                    fields[i][j].Height = y;
                    fields[i][j].Width = x;
                    fields[i][j].Click += zoom;
                    fields[i][j].MouseRightButtonUp += add;
                    Canvas.SetLeft(fields[i][j], j*x);
                    Canvas.SetTop(fields[i][j], i * y);
                    canvas.Children.Add(fields[i][j]);
                    var elipse = createEllipse(x, y, j * x, i * y);
                    ellipses[i][j] = elipse;
                }
            }
        }

        private void clean(object sender, RoutedEventArgs e)
        {
            _died = 0;
            _birth = 0;
            _turn = 0;
            Turn.Text = _turn.ToString();
            Died.Text = _died.ToString();
            Birth.Text = _birth.ToString();
            _isRunning = false;
            animButon.Background = Brushes.Red;
            for (int i =0; i < fields.Length; i++)
            {
                for(int j =0; j < fields[i].Length; j++)
                {
                    fields[i][j].Background = Brushes.White;
                    canvas.Children.Remove(ellipses[i][j]);
                }
            }
        }

        private void random(object sender, RoutedEventArgs e)
        {
            Random rand = new Random();
            for (int i = 0; i < fields.Length; i++)
            {
                for (int j = 0; j < fields[i].Length; j++)
                {
                    if (rand.NextDouble() < density.Value)
                        if (!_isCircle) fields[i][j].Background = Brushes.Black;
                        else canvas.Children.Add(ellipses[i][j]);
                }
            }
        }

        private void algorithm(int i, int j)
        {
            int count = 0;
            if(i > 0)
            {
                if(j > 0)
                {
                    count += fields[i - 1][j - 1].Background == Brushes.Black ? 1 : 0;
                    count += canvas.Children.Contains(ellipses[i - 1][j - 1]) ? 1 : 0;
                }
                count += fields[i - 1][j].Background == Brushes.Black ? 1 : 0;
                count += canvas.Children.Contains(ellipses[i - 1][j]) ? 1 : 0;
                if (j < fields[0].Length - 1)
                {
                    count += fields[i - 1][j + 1].Background == Brushes.Black ? 1 : 0;
                    count += canvas.Children.Contains(ellipses[i - 1][j + 1]) ? 1 : 0;
                }
            }
            if(j > 0)
            {
                count += fields[i][j - 1].Background == Brushes.Black ? 1 : 0;
                count += canvas.Children.Contains(ellipses[i][j - 1]) ? 1 : 0;
            }
            if(j < fields[0].Length - 1)
            {
                count += fields[i][j + 1].Background == Brushes.Black ? 1 : 0;
                count += canvas.Children.Contains(ellipses[i][j + 1]) ? 1 : 0;
            }
            if(i < fields.Length - 1)
            {
                if (j > 0)
                {
                    count += fields[i + 1][j - 1].Background == Brushes.Black ? 1 : 0;
                    count += canvas.Children.Contains(ellipses[i + 1][j - 1]) ? 1 : 0;
                }
                count += fields[i + 1][j].Background == Brushes.Black ? 1 : 0;
                count += canvas.Children.Contains(ellipses[i + 1][j]) ? 1 : 0;
                if (j < fields[0].Length - 1)
                {
                    count += fields[i + 1][j + 1].Background == Brushes.Black ? 1 : 0;
                    count += canvas.Children.Contains(ellipses[i + 1][j + 1]) ? 1 : 0;
                }
            }
            if ((fields[i][j].Background == Brushes.White && !_isCircle) || (_isCircle && !canvas.Children.Contains(ellipses[i][j])))
            {
                if (count == 3) newState[1][j] = true;
                else newState[1][j] = false;
            }
            else
            {
                if (count == 2 || count == 3) newState[1][j] = true;
                else newState[1][j] = false;
            }
        }

        private void stepAction(object sender, RoutedEventArgs e)
        {
            step();
        }


        private void step()
        {
            _turn++;
            for (int i =0; i < fields.Length; i++)
            {
                for(int j = 0; j < fields.Length; j++)
                {
                    algorithm(i, j);
                }
                if (i > 0)
                {
                    for(int j =0; j < fields[0].Length; j++)
                    {
                        if (newState[0][j])
                        {
                            if (fields[i - 1][j].Background != Brushes.Black && !_isCircle)
                            {
                                fields[i - 1][j].Background = Brushes.Black;
                                _birth++;
                            }
                            else if(_isCircle && !canvas.Children.Contains(ellipses[i - 1][j]))
                            {
                                canvas.Children.Add(ellipses[i - 1][j]);
                                _birth++;
                            }
                        }
                        else
                        {
                            if (fields[i - 1][j].Background != Brushes.White && !_isCircle)
                            {
                                fields[i - 1][j].Background = Brushes.White;
                                _died++;
                            }
                            else if (_isCircle && canvas.Children.Contains(ellipses[i - 1][j]))
                            {
                                canvas.Children.Remove(ellipses[(i - 1)][j]);
                                _died++;
                            }
                        }
                    }
                }
                Array.Copy(newState[1], newState[0], newState[0].Length);
            }
            for(int j =0; j < fields[0].Length; j++)
            {
                if (newState[0][j])
                {
                    if (fields[fields.Length - 1][j].Background != Brushes.Black && !_isCircle)
                    {
                        fields[fields.Length - 1][j].Background = Brushes.Black;
                        _birth++;
                    }
                    else if (_isCircle && !canvas.Children.Contains(ellipses[ellipses.Length - 1][j]))
                    {
                        canvas.Children.Add(ellipses[ellipses.Length - 1][j]);
                        _birth++;
                    }
                }
                else
                {
                    if (fields[fields.Length - 1][j].Background != Brushes.White && !_isCircle)
                    {
                        fields[fields.Length - 1][j].Background = Brushes.White;
                        _died++;
                    }
                    else if (_isCircle && canvas.Children.Contains(ellipses[ellipses.Length - 1][j]))
                    {
                        canvas.Children.Remove(ellipses[(ellipses.Length - 1)][j]);
                        _died++;
                    }
                }
            }
            Turn.Text = _turn.ToString();
            Died.Text = _died.ToString();
            Birth.Text = _birth.ToString();
        }

        private void save(object sender, RoutedEventArgs e)
        {
            save("save");
        }

        private void save(string name)
        {
            var file = File.CreateText($"{name}.gol");
            file.WriteLine($"hight: {fields.Length}");
            file.WriteLine($"width: {fields[0].Length}");
            for (int i = 0; i < fields.Length; i++)
            {
                for (int j = 0; j < fields[0].Length; j++)
                {
                    if (fields[i][j].Background == Brushes.Black || canvas.Children.Contains(ellipses[i][j]) )
                    {
                        file.WriteLine($"{i} {j}");
                    }
                }
            }
            file.Close();
        }

        private void read(string name)
        {
            if (name != "tmp")
            {
                _died = 0;
                _birth = 0;
                _turn = 0;
                Turn.Text = _turn.ToString();
                Died.Text = _died.ToString();
                Birth.Text = _birth.ToString();
            }
            var file = File.OpenText($"{name}.gol");
            var height = int.Parse(file.ReadLine().Substring(7));
            var width = int.Parse(file.ReadLine().Substring(7));
            List<(int, int)> values = new List<(int, int)>();
            var lines = file.ReadToEnd().Trim().Split("\r\n");
            file.Close();
            foreach (var line in lines)
            {
                if (line == "") break;
                var cords = line.Split(' ').Select(x => int.Parse(x)).ToArray();
                values.Add((cords[0], cords[1]));
            }



            canvas.Children.Clear();

            fields = new Button[height][];
            ellipses = new Ellipse[height][];

            for (int i = 0; i < height; i++)
            {
                fields[i] = new Button[width];
                ellipses[i] = new Ellipse[width];
            }

            newState[0] = new bool[width];
            newState[1] = new bool[width];

            double x = name == "tmp" ? canvas.Width / width : canvas.ActualWidth / width;
            double y = name == "tmp" ? canvas.Height / height : canvas.ActualHeight / height;

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    fields[i][j] = new Button();
                    fields[i][j].Name = "Button" + "A" + i.ToString() + "A" + j.ToString();
                    fields[i][j].Background = Brushes.White;
                    fields[i][j].Height = y;
                    fields[i][j].Width = x;
                    fields[i][j].Click += zoom;
                    fields[i][j].MouseRightButtonUp += add;
                    Canvas.SetLeft(fields[i][j], j * x);
                    Canvas.SetTop(fields[i][j], i * y);
                    canvas.Children.Add(fields[i][j]);
                    var elipse = createEllipse(x, y, j * x, i * y);
                    ellipses[i][j] = elipse;
                    if (values.Contains((i, j)))
                    {
                        if (_isCircle)
                        {
                            canvas.Children.Add(elipse);
                        }
                        else
                        {
                            fields[i][j].Background = Brushes.Black;
                        }
                    }
                }
            }
        }

        private Ellipse createEllipse(double width, double height, double left, double top)
        {
            var ellipse = new Ellipse();
            ellipse.Height = height;
            ellipse.Width = width;
            ellipse.Stroke = Brushes.Black;
            ellipse.StrokeThickness = 3;
            Canvas.SetLeft(ellipse, left);
            Canvas.SetTop(ellipse, top);
            return ellipse;
        }

        private void read(object sender, RoutedEventArgs e)
        {
            read("save");
        }

        private void mode(object sender, RoutedEventArgs e)
        {
            if (_isCircle)
            {
                for (int i = 0; i < ellipses.Length; i++)
                {
                    for (int j = 0; j < ellipses[0].Length; j++)
                    {
                        if (canvas.Children.Contains(ellipses[i][j]))
                        {
                            fields[i][j].Background = Brushes.Black;
                            canvas.Children.Remove(ellipses[i][j]);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < fields.Length; i++)
                {
                    for (int j = 0; j < fields[0].Length; j++)
                    {
                        if (fields[i][j].Background == Brushes.Black)
                        {
                            fields[i][j].Background = Brushes.White;
                            canvas.Children.Add(ellipses[i][j]);
                        }
                    }
                }
            }
            _isCircle = !_isCircle;
        }
    }
}