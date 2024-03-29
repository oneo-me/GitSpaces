using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using GitSpaces.Models;

namespace GitSpaces.Converters;

public static class DecoratorTypeConverters
{
    public static FuncValueConverter<DecoratorType, IBrush> ToBackground =
        new(v =>
        {
            if (v == DecoratorType.Tag) return DecoratorResources.Backgrounds[0];
            return DecoratorResources.Backgrounds[1];
        });

    public static FuncValueConverter<DecoratorType, StreamGeometry> ToIcon =
        new(v =>
        {
            var key = "Icons.Tag";
            switch (v)
            {
                case DecoratorType.CurrentBranchHead:
                    key = "Icons.Check";
                    break;

                case DecoratorType.RemoteBranchHead:
                    key = "Icons.Remote";
                    break;

                case DecoratorType.LocalBranchHead:
                    key = "Icons.Branch";
                    break;
            }

            return Application.Current?.FindResource(key) as StreamGeometry;
        });
}
