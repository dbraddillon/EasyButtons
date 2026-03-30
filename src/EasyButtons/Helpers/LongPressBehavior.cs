using Android.Views;
using System.Windows.Input;

namespace EasyButtons.Helpers;

/// <summary>
/// Handles both tap (→ TapCommand) and long press (→ LongPressCommand) via a single
/// native GestureDetector. Replaces MAUI TapGestureRecognizer on the same view so the
/// touch listener isn't split between two systems.
/// </summary>
public class ButtonGestureBehavior : Behavior<Microsoft.Maui.Controls.View>
{
    public static readonly BindableProperty TapCommandProperty =
        BindableProperty.Create(nameof(TapCommand), typeof(ICommand), typeof(ButtonGestureBehavior));
    public static readonly BindableProperty TapCommandParameterProperty =
        BindableProperty.Create(nameof(TapCommandParameter), typeof(object), typeof(ButtonGestureBehavior));
    public static readonly BindableProperty LongPressCommandProperty =
        BindableProperty.Create(nameof(LongPressCommand), typeof(ICommand), typeof(ButtonGestureBehavior));
    public static readonly BindableProperty LongPressCommandParameterProperty =
        BindableProperty.Create(nameof(LongPressCommandParameter), typeof(object), typeof(ButtonGestureBehavior));

    public ICommand? TapCommand           { get => (ICommand?)GetValue(TapCommandProperty);           set => SetValue(TapCommandProperty, value); }
    public object?   TapCommandParameter  { get => GetValue(TapCommandParameterProperty);              set => SetValue(TapCommandParameterProperty, value); }
    public ICommand? LongPressCommand     { get => (ICommand?)GetValue(LongPressCommandProperty);     set => SetValue(LongPressCommandProperty, value); }
    public object?   LongPressCommandParameter { get => GetValue(LongPressCommandParameterProperty); set => SetValue(LongPressCommandParameterProperty, value); }

    private Android.Views.View?               _native;
    private GestureDetector?                  _detector;
    private Microsoft.Maui.Controls.View?     _mauiView;

    protected override void OnAttachedTo(Microsoft.Maui.Controls.View bindable)
    {
        base.OnAttachedTo(bindable);
        _mauiView  = bindable;
        BindingContext = bindable.BindingContext;
        bindable.BindingContextChanged += OnBindingContextChanged;
        bindable.HandlerChanged        += OnHandlerChanged;
        if (bindable.Handler?.PlatformView is Android.Views.View native)
            Attach(native);
    }

    protected override void OnDetachingFrom(Microsoft.Maui.Controls.View bindable)
    {
        base.OnDetachingFrom(bindable);
        bindable.BindingContextChanged -= OnBindingContextChanged;
        bindable.HandlerChanged        -= OnHandlerChanged;
        _mauiView = null;
        Detach();
    }

    private void OnBindingContextChanged(object? sender, EventArgs e)
    {
        if (sender is Microsoft.Maui.Controls.View v) BindingContext = v.BindingContext;
    }

    private void OnHandlerChanged(object? sender, EventArgs e)
    {
        Detach();
        if ((sender as Microsoft.Maui.Controls.View)?.Handler?.PlatformView is Android.Views.View native)
            Attach(native);
    }

    private void Attach(Android.Views.View native)
    {
        _native   = native;
        _detector = new GestureDetector(native.Context!, new Listener(this));
        native.SetOnTouchListener(new TouchForwarder(_detector));
    }

    private void Detach()
    {
        _native?.SetOnTouchListener(null);
        _native   = null;
        _detector = null;
    }

    internal void FireTap()
    {
        var view       = _mauiView;
        var (cmd, p)   = (TapCommand, TapCommandParameter);
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            // Press-in animation, then spring back while command runs
            if (view != null)
            {
                await view.ScaleToAsync(0.86, 70, Easing.CubicIn);
                _ = view.ScaleToAsync(1.0, 200, Easing.SpringOut);
            }
            if (cmd?.CanExecute(p) == true) cmd.Execute(p);
        });
    }

    internal void FireLongPress()
    {
        var view       = _mauiView;
        var (cmd, p)   = (LongPressCommand, LongPressCommandParameter);
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (view != null)
            {
                await view.ScaleToAsync(0.92, 80, Easing.CubicIn);
                _ = view.ScaleToAsync(1.0, 160, Easing.SpringOut);
            }
            if (cmd?.CanExecute(p) == true) cmd.Execute(p);
        });
    }

    // ── inner classes ────────────────────────────────────────────────────────

    private class Listener : GestureDetector.SimpleOnGestureListener
    {
        private readonly ButtonGestureBehavior _b;
        public Listener(ButtonGestureBehavior b) => _b = b;
        public override bool OnDown(MotionEvent e)         => true; // must return true or UP never arrives
        public override bool OnSingleTapUp(MotionEvent e) { _b.FireTap(); return true; }
        public override void OnLongPress(MotionEvent e)   => _b.FireLongPress();
    }

    private class TouchForwarder : Java.Lang.Object, Android.Views.View.IOnTouchListener
    {
        private readonly GestureDetector _d;
        public TouchForwarder(GestureDetector d) => _d = d;
        public bool OnTouch(Android.Views.View? v, MotionEvent? e) => _d.OnTouchEvent(e!);
    }
}
