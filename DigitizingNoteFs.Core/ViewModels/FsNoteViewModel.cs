﻿using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace DigitizingNoteFs.Core.ViewModels
{
    public partial class FsNoteViewModel : ObservableObject
    {
        public string? CellAddress { get; set; }
        public Tuple<int, int>? Cell { get; set; }

        [ObservableProperty]
        private string? name;

        [ObservableProperty]
        private double value;

        partial void OnValueChanged(double oldValue, double newValue)
        {
            
        }
        
        [ObservableProperty]
        private int fsNoteId;

        [ObservableProperty]
        private int parentId;

        [ObservableProperty]
        private bool isParent;

        [ObservableProperty]
        private int group;

        [ObservableProperty]
        public bool isSelected;
    }

    public partial class FsNoteParentViewModel : FsNoteViewModel
    {
        [ObservableProperty]
        private bool isValid;

        public List<FsNoteViewModel> Children { get; set; } = [];
    }
}
