using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using GitSpaces.Models;

namespace GitSpaces.OldViews;

public class Avatar : Control, IAvatarHost
{
    static readonly GradientStops[] FALLBACK_GRADIENTS =
    [
        new()
        {
            new(Colors.Orange, 0), new(Color.FromRgb(255, 213, 134), 1)
        },
        new()
        {
            new(Colors.DodgerBlue, 0), new(Colors.LightSkyBlue, 1)
        },
        new()
        {
            new(Colors.LimeGreen, 0), new(Color.FromRgb(124, 241, 124), 1)
        },
        new()
        {
            new(Colors.Orchid, 0), new(Color.FromRgb(248, 161, 245), 1)
        },
        new()
        {
            new(Colors.Tomato, 0), new(Color.FromRgb(252, 165, 150), 1)
        }
    ];

    public static readonly StyledProperty<User> UserProperty =
        AvaloniaProperty.Register<Avatar, User>(nameof(User));

    public User User
    {
        get => GetValue(UserProperty);
        set => SetValue(UserProperty, value);
    }

    static Avatar()
    {
        UserProperty.Changed.AddClassHandler<Avatar>(OnUserPropertyChanged);
    }

    public Avatar()
    {
        var refetch = new MenuItem
        {
            Header = App123.Text("RefetchAvatar")
        };
        refetch.Click += (o, e) =>
        {
            if (User != null)
            {
                AvatarManager.Request(_emailMD5, true);
                InvalidateVisual();
            }
        };

        ContextMenu = new();
        ContextMenu.Items.Add(refetch);
    }

    public override void Render(DrawingContext context)
    {
        if (User == null) return;

        var corner = (float)Math.Max(2, Bounds.Width / 16);
        var img = AvatarManager.Request(_emailMD5);
        if (img != null)
        {
            var rect = new Rect(0, 0, Bounds.Width, Bounds.Height);
            context.PushClip(new RoundedRect(rect, corner));
            context.DrawImage(img, rect);
        }
        else
        {
            var textOrigin = new Point((Bounds.Width - _fallbackLabel.Width) * 0.5, (Bounds.Height - _fallbackLabel.Height) * 0.5);
            context.DrawRectangle(_fallbackBrush, null, new(0, 0, Bounds.Width, Bounds.Height), corner, corner);
            context.DrawText(_fallbackLabel, textOrigin);
        }
    }

    public void OnAvatarResourceChanged(string md5)
    {
        if (_emailMD5 == md5)
        {
            InvalidateVisual();
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        AvatarManager.Subscribe(this);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        AvatarManager.Unsubscribe(this);
    }

    static void OnUserPropertyChanged(Avatar avatar, AvaloniaPropertyChangedEventArgs e)
    {
        if (avatar.User == null)
        {
            avatar._emailMD5 = null;
            return;
        }

        var placeholder = string.IsNullOrWhiteSpace(avatar.User.Name) ? "?" : avatar.User.Name.Substring(0, 1);
        var chars = placeholder.ToCharArray();
        var sum = 0;
        foreach (var c in chars) sum += Math.Abs(c);

        var hash = MD5.Create().ComputeHash(Encoding.Default.GetBytes(avatar.User.Email.ToLower().Trim()));
        var builder = new StringBuilder();
        foreach (var c in hash) builder.Append(c.ToString("x2"));
        var md5 = builder.ToString();
        if (avatar._emailMD5 != md5) avatar._emailMD5 = md5;

        avatar._fallbackBrush = new()
        {
            GradientStops = FALLBACK_GRADIENTS[sum % FALLBACK_GRADIENTS.Length], StartPoint = new(0, 0, RelativeUnit.Relative), EndPoint = new(0, 1, RelativeUnit.Relative)
        };

        var typeface = new Typeface("fonts:GitSpaces#JetBrains Mono");

        avatar._fallbackLabel = new(
            placeholder,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            avatar.Width * 0.65,
            Brushes.White);

        avatar.InvalidateVisual();
    }

    FormattedText _fallbackLabel;
    LinearGradientBrush _fallbackBrush;
    string _emailMD5;
}
