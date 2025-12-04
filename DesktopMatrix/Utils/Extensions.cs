using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DesktopMatrix.Models;
namespace DesktopMatrix.Utils
{
    /// <summary>
    /// 扩展方法类
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// 将普通集合转换为ObservableCollection
        /// </summary>
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> collection)
        {
            return new ObservableCollection<T>(collection);
        }

        /// <summary>
        /// 获取象限类型的显示名称
        /// </summary>
        public static string GetDisplayName(this QuadrantType quadrantType)
        {
            return Constants.QuadrantDescriptions[quadrantType];
        }

        /// <summary>
        /// 获取象限类型对应的背景色
        /// </summary>
        public static SolidColorBrush GetQuadrantBrush(this QuadrantType quadrantType)
        {
            switch (quadrantType)
            {
                case QuadrantType.Q1:
                    return new SolidColorBrush(Colors.IndianRed);
                case QuadrantType.Q2:
                    return new SolidColorBrush(Colors.SteelBlue);
                case QuadrantType.Q3:
                    return new SolidColorBrush(Colors.Orange);
                case QuadrantType.Q4:
                    return new SolidColorBrush(Colors.LightGreen);
                default:
                    return new SolidColorBrush(Colors.Gray);
            }
        }

        /// <summary>
        /// 查找父控件
        /// </summary>
        public static T FindParent<T>(this DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
                return null;

            if (parentObject is T parent)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

        /// <summary>
        /// 格式化时间显示
        /// </summary>
        public static string ToFriendlyTimeString(this DateTime dateTime)
        {
            TimeSpan timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalDays > 30)
            {
                return dateTime.ToString("yyyy-MM-dd");
            }
            else if (timeSpan.TotalDays > 1)
            {
                return $"{(int)timeSpan.TotalDays}天前";
            }
            else if (timeSpan.TotalHours > 1)
            {
                return $"{(int)timeSpan.TotalHours}小时前";
            }
            else if (timeSpan.TotalMinutes > 1)
            {
                return $"{(int)timeSpan.TotalMinutes}分钟前";
            }
            else
            {
                return "刚刚";
            }
        }
    }
}