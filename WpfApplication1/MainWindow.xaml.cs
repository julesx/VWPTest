using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DynamicData;
using DynamicData.Binding;

namespace WpfApplication1
{
    public class Sorter : AbstractNotifyPropertyChanged, IComparer<string>
    {
        public Sorter()
        {
            
        }
        private bool _sortAscending = true;

        public bool SortAscending
        {
            get { return _sortAscending; }
            set
            {
                _sortAscending = value;
                OnPropertyChanged();
            }
        }

        public int Compare(string x, string y)
        {
            if (SortAscending)
                return string.Compare(x, y, StringComparison.Ordinal);
            else
                return string.Compare(y, x, StringComparison.Ordinal);
        }
    }

    public partial class MainWindow : Window
    {
        public IObservableCollection<string> DdItems { get; } = new ObservableCollectionExtended<string>();
        public ObservableCollection<string> RegItems { get; } = new ObservableCollection<string>();

        public SourceCache<string, string> SourceCache { get; } = new SourceCache<string, string>(x => x);

        private readonly Sorter _sorter;

        public MainWindow()
        {
            InitializeComponent();

            var items = new List<string>() {"item 1", "item 2"};

            RegItems.AddRange(items);

            SourceCache.AddOrUpdate(items);

            _sorter = new Sorter();

            var sortControlChanged = _sorter.WhenAnyPropertyChanged();
            var sorter = sortControlChanged
                .StartWith(_sorter);

            var loader = SourceCache
                        .Connect()
                        .Sort(sorter)
                        .ObserveOnDispatcher()
                        .Bind(DdItems)         //Populate the observable collection
                        .DisposeMany()          //Dispose TradeProxy when no longer required
                        .Subscribe();

            DataContext = this;
        }

        private bool _sortedAscending = true;

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;

            _sortedAscending = !_sortedAscending;

            RegItems.Sort(_sortedAscending);
            _sorter.SortAscending = !_sorter.SortAscending;

            if (_sortedAscending)
                button.Content = "Sorted Ascending";
            else
                button.Content = "Sorted Descending";
        }
    }

    public static class Extensions
    {
        public static void Sort<T>(this ObservableCollection<T> observable, bool sortAscending) where T : IComparable<T>, IEquatable<T>
        {
            List<T> sorted = sortAscending ? observable.OrderBy(x => x).ToList() : observable.OrderByDescending(x => x).ToList();

            int ptr = 0;
            while (ptr < sorted.Count)
            {
                if (!observable[ptr].Equals(sorted[ptr]))
                {
                    T t = observable[ptr];
                    observable.RemoveAt(ptr);
                    observable.Insert(sorted.IndexOf(t), t);
                }
                else
                {
                    ptr++;
                }
            }
        }
    }
}
