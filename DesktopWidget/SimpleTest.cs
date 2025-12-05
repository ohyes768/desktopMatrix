using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DesktopWidget.Models;

namespace DesktopWidget.Services
{
    // 简单的内存版本，用于测试UI逻辑
    public class SimpleTaskManager
    {
        public ObservableCollection<TaskItem> Q1Tasks { get; private set; }
        public ObservableCollection<TaskItem> Q2Tasks { get; private set; }
        public ObservableCollection<TaskItem> Q3Tasks { get; private set; }
        public ObservableCollection<TaskItem> Q4Tasks { get; private set; }

        private static int _nextId = 1;

        public SimpleTaskManager()
        {
            Q1Tasks = new ObservableCollection<TaskItem>();
            Q2Tasks = new ObservableCollection<TaskItem>();
            Q3Tasks = new ObservableCollection<TaskItem>();
            Q4Tasks = new ObservableCollection<TaskItem>();
        }

        public TaskItem AddTask(string title, QuadrantType quadrant)
        {
            try
            {
                var task = new TaskItem(title, quadrant);
                task.Id = _nextId++;

                AddTaskToQuadrant(task);
                return task;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SimpleTaskManager.AddTask 失败: {ex.Message}");
                return null;
            }
        }

        private void AddTaskToQuadrant(TaskItem task)
        {
            switch (task.Quadrant)
            {
                case QuadrantType.Q1:
                    Q1Tasks.Add(task);
                    break;
                case QuadrantType.Q2:
                    Q2Tasks.Add(task);
                    break;
                case QuadrantType.Q3:
                    Q3Tasks.Add(task);
                    break;
                case QuadrantType.Q4:
                    Q4Tasks.Add(task);
                    break;
            }
        }
    }
}