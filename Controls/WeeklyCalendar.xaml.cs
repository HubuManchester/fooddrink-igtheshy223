using System.Windows.Input;

namespace ssk.Controls;

public partial class WeeklyCalendar : ContentView
{
    private readonly Button[] _dayButtons;
    private readonly BoxView[] _dots;
    private DateTime[] _weekDates = new DateTime[7];

    public static readonly BindableProperty SelectedDateProperty =
        BindableProperty.Create(nameof(SelectedDate), typeof(DateTime), typeof(WeeklyCalendar), DateTime.Today, propertyChanged: OnDateChanged);

    public static readonly BindableProperty DatesWithPlansProperty =
        BindableProperty.Create(nameof(DatesWithPlans), typeof(IEnumerable<string>), typeof(WeeklyCalendar), null, propertyChanged: OnDateChanged);

    public static readonly BindableProperty DateSelectedCommandProperty =
        BindableProperty.Create(nameof(DateSelectedCommand), typeof(ICommand), typeof(WeeklyCalendar));

    public DateTime SelectedDate
    {
        get => (DateTime)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    public IEnumerable<string>? DatesWithPlans
    {
        get => (IEnumerable<string>?)GetValue(DatesWithPlansProperty);
        set => SetValue(DatesWithPlansProperty, value);
    }

    public ICommand? DateSelectedCommand
    {
        get => (ICommand?)GetValue(DateSelectedCommandProperty);
        set => SetValue(DateSelectedCommandProperty, value);
    }

    public WeeklyCalendar()
    {
        InitializeComponent();
        _dayButtons = new[] { Day0, Day1, Day2, Day3, Day4, Day5, Day6 };
        _dots = new[] { Dot0, Dot1, Dot2, Dot3, Dot4, Dot5, Dot6 };
        UpdateDisplay();
    }

    private static void OnDateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((WeeklyCalendar)bindable).UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        var selected = SelectedDate.Date;
        int diff = (int)selected.DayOfWeek - (int)DayOfWeek.Monday;
        if (diff < 0) diff += 7;
        var monday = selected.AddDays(-diff);

        System.Diagnostics.Debug.WriteLine($"[WeeklyCalendar] SelectedDate={selected:yyyy-MM-dd}, monday={monday:yyyy-MM-dd}, diff={diff}");

        var plansSet = DatesWithPlans?.ToHashSet() ?? new HashSet<string>();
        var primary = Color.FromArgb("#6366F1");
        var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;

        for (int i = 0; i < 7; i++)
        {
            var d = monday.AddDays(i);
            _weekDates[i] = d;

            var isToday = d == DateTime.Today;
            var isSelected = d == selected;

            _dayButtons[i].Text = d.Day.ToString();

            System.Diagnostics.Debug.WriteLine($"[WeeklyCalendar] Day[{i}]: date={d:yyyy-MM-dd}, day={d.Day}, text={_dayButtons[i].Text}");

            if (isSelected || isToday)
            {
                _dayButtons[i].BackgroundColor = primary;
                _dayButtons[i].TextColor = Colors.White;
                _dayButtons[i].FontAttributes = FontAttributes.Bold;
            }
            else
            {
                _dayButtons[i].BackgroundColor = Colors.Transparent;
                _dayButtons[i].TextColor = isDark ? Colors.White : Colors.Black;
                _dayButtons[i].FontAttributes = FontAttributes.None;
            }

            _dots[i].BackgroundColor = plansSet.Contains(d.ToString("yyyy-MM-dd"))
                ? primary : Colors.Transparent;
        }
    }

    private void OnDayClicked(object? sender, EventArgs e)
    {
        if (sender is not Button btn) return;
        int idx = Array.IndexOf(_dayButtons, btn);
        if (idx < 0 || idx >= 7) return;

        var clickedDate = _weekDates[idx];
        SelectedDate = clickedDate;
        DateSelectedCommand?.Execute(clickedDate);
    }
}
