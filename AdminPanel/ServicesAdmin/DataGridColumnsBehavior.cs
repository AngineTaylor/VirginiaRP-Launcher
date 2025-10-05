using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace AdminPanelWPF.ServicesAdmin
{
    public static class DataGridColumnsBehavior
    {
        public static readonly DependencyProperty ExtraColumnsProperty =
            DependencyProperty.RegisterAttached(
                "ExtraColumns",
                typeof(IEnumerable),
                typeof(DataGridColumnsBehavior),
                new PropertyMetadata(null, OnExtraColumnsChanged));

        public static IEnumerable GetExtraColumns(DependencyObject obj)
        {
            return (IEnumerable)obj.GetValue(ExtraColumnsProperty);
        }

        public static void SetExtraColumns(DependencyObject obj, IEnumerable value)
        {
            obj.SetValue(ExtraColumnsProperty, value);
        }

        private static void OnExtraColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                // Очищаем предыдущие колонки
                if (e.OldValue is INotifyCollectionChanged oldCollection)
                {
                    oldCollection.CollectionChanged -= (s, args) => OnCollectionChanged(dataGrid, args);
                }

                // Удаляем все динамически добавленные колонки
                var columnsToRemove = new System.Collections.Generic.List<DataGridColumn>();
                foreach (var column in dataGrid.Columns)
                {
                    if (column.GetValue(IsExtraColumnProperty) is bool isExtra && isExtra)
                    {
                        columnsToRemove.Add(column);
                    }
                }

                foreach (var column in columnsToRemove)
                {
                    dataGrid.Columns.Remove(column);
                }

                // Добавляем новые колонки
                if (e.NewValue is IEnumerable newColumns)
                {
                    foreach (var item in newColumns)
                    {
                        if (item is DataGridColumn column)
                        {
                            column.SetValue(IsExtraColumnProperty, true);
                            dataGrid.Columns.Add(column);
                        }
                    }
                }

                // Подписываемся на изменения
                if (e.NewValue is INotifyCollectionChanged newCollection)
                {
                    newCollection.CollectionChanged += (s, args) => OnCollectionChanged(dataGrid, args);
                }
            }
        }

        private static readonly DependencyProperty IsExtraColumnProperty =
            DependencyProperty.RegisterAttached("IsExtraColumn", typeof(bool), typeof(DataGridColumnsBehavior));

        private static void OnCollectionChanged(DataGrid dataGrid, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                // Удаляем все extra колонки
                var columnsToRemove = new System.Collections.Generic.List<DataGridColumn>();
                foreach (var column in dataGrid.Columns)
                {
                    if (column.GetValue(IsExtraColumnProperty) is bool isExtra && isExtra)
                    {
                        columnsToRemove.Add(column);
                    }
                }

                foreach (var column in columnsToRemove)
                {
                    dataGrid.Columns.Remove(column);
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is DataGridColumn column && dataGrid.Columns.Contains(column))
                    {
                        dataGrid.Columns.Remove(column);
                    }
                }
            }

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is DataGridColumn column && !dataGrid.Columns.Contains(column))
                    {
                        column.SetValue(IsExtraColumnProperty, true);
                        dataGrid.Columns.Add(column);
                    }
                }
            }
        }
    }
}