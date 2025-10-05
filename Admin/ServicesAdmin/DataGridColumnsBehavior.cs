using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace Admin.ServicesAdmin
{
    public static class DataGridColumnsBehavior
    {
        public static readonly DependencyProperty ExtraColumnsProperty =
            DependencyProperty.RegisterAttached(
                "ExtraColumns",
                typeof(IEnumerable),
                typeof(DataGridColumnsBehavior),
                new PropertyMetadata(null, OnExtraColumnsChanged));

        public static IEnumerable GetExtraColumns(DataGrid obj)
        {
            return (IEnumerable)obj.GetValue(ExtraColumnsProperty);
        }

        public static void SetExtraColumns(DataGrid obj, IEnumerable value)
        {
            obj.SetValue(ExtraColumnsProperty, value);
        }

        private static void OnExtraColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = d as DataGrid;
            if (dataGrid == null) return;

            // Отписываем старую коллекцию
            var oldCollection = e.OldValue as INotifyCollectionChanged;
            if (oldCollection != null)
            {
                oldCollection.CollectionChanged -= OldCollectionChangedHandler;
            }

            // Подписываемся на новую коллекцию
            var newCollection = e.NewValue as INotifyCollectionChanged;
            if (newCollection != null)
            {
                newCollection.CollectionChanged += NewCollectionChangedHandler;
            }

            // Сохраняем DataGrid в Tag для использования в обработчиках
            dataGrid.Tag = dataGrid.Tag ?? new HashSet<DataGridColumn>();

            // Обновляем колонки сразу
            UpdateColumns(dataGrid, e.NewValue as IEnumerable);
        }

        // Обработчик для старой коллекции
        private static void OldCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            var dataGrid = sender as DataGrid ?? FindDataGridFromSender(sender);
            if (dataGrid != null)
            {
                UpdateColumns(dataGrid, GetExtraColumns(dataGrid));
            }
        }

        // Обработчик для новой коллекции
        private static void NewCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            var dataGrid = sender as DataGrid ?? FindDataGridFromSender(sender);
            if (dataGrid != null)
            {
                UpdateColumns(dataGrid, GetExtraColumns(dataGrid));
            }
        }

        private static DataGrid FindDataGridFromSender(object sender)
        {
            // Если нужно, здесь можно добавить поиск DataGrid по sender
            return null;
        }

        private static void UpdateColumns(DataGrid dataGrid, IEnumerable newColumns)
        {
            var extraColumnsSet = dataGrid.Tag as HashSet<DataGridColumn>;
            if (extraColumnsSet == null)
            {
                extraColumnsSet = new HashSet<DataGridColumn>();
                dataGrid.Tag = extraColumnsSet;
            }

            // Удаляем старые дополнительные колонки
            foreach (var col in extraColumnsSet)
            {
                dataGrid.Columns.Remove(col);
            }
            extraColumnsSet.Clear();

            if (newColumns == null) return;

            // Добавляем новые дополнительные колонки
            foreach (DataGridColumn col in newColumns)
            {
                dataGrid.Columns.Add(col);
                extraColumnsSet.Add(col);
            }
        }
    }
}
