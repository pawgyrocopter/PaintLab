using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.Windows.Shapes.Path;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private enum Shapes
        {
            Rectangle,
            Circle,
            Line,
            Path,
            Draw
        }

        private Shapes _shape;
        private bool _mouseDown;
        private Rectangle _rect;
        private Ellipse _circle;
        private Line _line;
        private Point _point;
        private Polyline _polyline;
        private Path _path;
        private bool _isPath = false;
        private SolidColorBrush _stroke = new SolidColorBrush(Colors.Red);
        private SolidColorBrush _fill = new SolidColorBrush(Colors.Red);

        private List<UIElement> _itemsList = new List<UIElement>();
        private List<UIElement> _itemsToRestore = new List<UIElement>();
        private int _itemNumber = 1;
        private Point _pathStartPoint;
        private int _selectedWidth = 5;
        private ObservableCollection<int> widths = new ObservableCollection<int>() {1, 2, 3, 4, 5, 6, 7, 8, 9, 10};

        public MainWindow()
        {
            InitializeComponent();

            ComboStroke.ItemsSource = typeof(Colors).GetProperties();
            ComboFill.ItemsSource = typeof(Colors).GetProperties();
            ComboWidth.ItemsSource = widths;
        }

        private void ComboStroke_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedColor = (Color) (ComboStroke.SelectedItem as PropertyInfo).GetValue(null, null);
            _stroke = new SolidColorBrush(selectedColor);
        }

        private void ComboWidth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedWidth = (int) ComboWidth.SelectedItem;
        }

        private void ComboFill_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedColor = (Color) (ComboFill.SelectedItem as PropertyInfo).GetValue(null, null);
            _fill = new SolidColorBrush(selectedColor);
        }

        private void DrawRectangle_Click(object sender, RoutedEventArgs e)
        {
            _shape = Shapes.Rectangle;
            _isPath = false;
        }

        private void DrawCircle_Click(object sender, RoutedEventArgs e)
        {
            _shape = Shapes.Circle;
            _isPath = false;
        }

        private void DrawLine_Click(object sender, RoutedEventArgs e)
        {
            _shape = Shapes.Line;
            _isPath = false;
        }

        private void DrawPath_Click(object sender, RoutedEventArgs e)
        {
            _shape = Shapes.Path;
            _isPath = false;
        }

        private void Draw_Click(object sender, RoutedEventArgs e)
        {
            _shape = Shapes.Draw;
            _isPath = false;
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            if (myCanvas.Children.Count == 0) return;
            myCanvas.Children.Remove(_itemsList[^1]);
            _itemsToRestore.Add(_itemsList[^1]);
            _itemsList.Remove(_itemsList[^1]);
        }

        private void Restore_Click(object sender, RoutedEventArgs e)
        {
            if (_itemsToRestore.Count == 0) return;
            myCanvas.Children.Add(_itemsToRestore[^1]);
            _itemsList.Add(_itemsToRestore[^1]);
            _itemsToRestore.Remove(_itemsToRestore[^1]);
        }

        /* Drawing events, change depending on selected shape. */

        private void myCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _mouseDown = (e.ButtonState == MouseButtonState.Pressed) && (e.ChangedButton == MouseButton.Left);
            if (!_mouseDown) return;
            switch (_shape)
            {
                case Shapes.Rectangle:
                {
                    _point = e.GetPosition(myCanvas);
                    _rect = new Rectangle
                    {
                        StrokeThickness = _selectedWidth,
                        Fill = _fill,
                        Stroke = _stroke,
                        Uid = $"item{_itemNumber}"
                    };
                    _itemsList.Add(_rect);
                    myCanvas.Children.Add(_rect);
                    break;
                }

                case Shapes.Circle:
                {
                    _point = e.GetPosition(myCanvas);
                    _circle = new Ellipse
                    {
                        StrokeThickness = 6,
                        Fill = _fill,
                        Stroke = _stroke,
                        Uid = $"item{_itemNumber}"
                    };
                    _itemsList.Add(_circle);
                    myCanvas.Children.Add(_circle);
                    break;
                }

                case Shapes.Line:
                {
                    _point = e.GetPosition(myCanvas);
                    _line = new Line
                    {
                        X1 = _point.X,
                        Y1 = _point.Y,
                        StrokeThickness = 6,
                        Stroke = _stroke,
                        Uid = $"item{_itemNumber}"
                    };
                    _itemsList.Add(_line);
                    myCanvas.Children.Add(_line);
                    break;
                }

                case Shapes.Path:
                {
                    if (_isPath)
                    {
                        _line = new Line
                        {
                            X1 = _pathStartPoint.X,
                            Y1 = _pathStartPoint.Y,
                            StrokeThickness = 6,
                            Stroke = _stroke,
                            Uid = $"item{_itemNumber}"
                        };
                        _itemsList.Add(_line);
                        myCanvas.Children.Add(_line);
                        break;
                    }
                    _point = e.GetPosition(myCanvas);
                    _line = new Line
                    {
                        X1 = _point.X,
                        Y1 = _point.Y,
                        StrokeThickness = 6,
                        Stroke = _stroke,
                        Uid = $"item{_itemNumber}"
                    };
                    _itemsList.Add(_line);
                    myCanvas.Children.Add(_line);
                    _isPath = true;
                    break;
                }

                case Shapes.Draw:
                {
                    _polyline = new Polyline
                    {
                        StrokeDashCap = PenLineCap.Round,
                        StrokeLineJoin = PenLineJoin.Round,
                        StrokeThickness = 6,
                        Stroke = _stroke,
                        Uid = $"item{_itemNumber}"
                    };
                    _itemsList.Add(_polyline);
                    myCanvas.Children.Add(_polyline);
                    break;
                }
                default:
                    return;
            }
        }

        [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH", MessageId = "type: System.Double")]
        private void myCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_mouseDown) return;
            switch (_shape)
            {
                case Shapes.Rectangle:
                {
                    var pos = e.GetPosition(myCanvas);
                    _rect.SetValue(Canvas.LeftProperty, Math.Min(pos.X, _point.X));
                    _rect.SetValue(Canvas.TopProperty, Math.Min(pos.Y, _point.Y));
                    _rect.Width = Math.Abs(pos.X - _point.X);
                    _rect.Height = Math.Abs(pos.Y - _point.Y);
                    break;
                }

                case Shapes.Circle:
                {
                    var pos = e.GetPosition(myCanvas);
                    _circle.SetValue(Canvas.LeftProperty, Math.Min(pos.X, _point.X));
                    _circle.SetValue(Canvas.TopProperty, Math.Min(pos.Y, _point.Y));
                    _circle.Width = Math.Abs(pos.X - _point.X);
                    _circle.Height = Math.Abs(pos.X - _point.X);
                    break;
                }

                case Shapes.Line:
                {
                    var pos = e.GetPosition(myCanvas);
                    _line.X2 = Math.Abs(pos.X);
                    _line.Y2 = Math.Abs(pos.Y);
                    break;
                }

                case Shapes.Path:
                {
                    var pos = e.GetPosition(myCanvas);
                    _line.X2 = Math.Abs(pos.X);
                    _line.Y2 = Math.Abs(pos.Y);
                    _pathStartPoint = pos;
                    break;
                }

                case Shapes.Draw:
                {
                    _polyline.Points.Add(e.GetPosition(myCanvas));
                    break;
                }
                default:
                    return;
            }
        }

        private void myCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;
            _mouseDown = false;
        }

        private void ClearCanvas(object sender, RoutedEventArgs e)
        {
            myCanvas.Children.Clear();
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            FileStream fs = File.Open(@"C:\Users\kR9_h\Desktop\WpfApp1\WpfApp1\newfile.xaml", FileMode.Create, FileAccess.Write);
            XamlWriter.Save(myCanvas, fs);
            fs.Close();
        }

        private void load_Click(object sender, RoutedEventArgs e)
        {
            FileStream fs = File.Open(@"C:\Users\kR9_h\Desktop\WpfApp1\WpfApp1\newfile.xaml", FileMode.Open,FileAccess.Read);
            Canvas savedCanvas = XamlReader.Load(fs) as Canvas;
            fs.Close();
            savedCanvas.Uid = $"items{_itemNumber}";
            myCanvas.Children.Add(savedCanvas);
        }
    }
}