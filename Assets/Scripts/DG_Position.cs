using System;
using System.ComponentModel;
using System.Globalization;
using Newtonsoft.Json;
using UnityEngine;

[TypeConverter(typeof(DG_PositionConverter))]
public struct DG_Position {
    [JsonIgnore]
    public static DG_Position zero = new DG_Position(0, 0);

    public int x;
    public int y;

    public DG_Position(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public DG_Position(Vector2 vector2) {
        this.x = (int) vector2.x;
        this.y = (int) vector2.y;
    }

    public DG_Position(Vector3 vector3) {
        this.x = (int) vector3.x;
        this.y = (int) vector3.z;
    }

    public override string ToString() {
        return x.ToString() + "," + y.ToString();
    }
}

public class DG_PositionConverter : TypeConverter {
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type source) {
        return source == typeof(string);
    }

    public override object ConvertFrom(ITypeDescriptorContext context,
        CultureInfo culture, object value) {

        var positionString = (string) value;

        var split = positionString.Split(',');

        return new DG_Position(int.Parse(split[0]), int.Parse(split[1]));
    }

    public override object ConvertTo(ITypeDescriptorContext context,
        CultureInfo culture,
        object value, Type destinationType) {

        var position = (DG_Position) value;

        return position.x.ToString() + "," + position.y.ToString();
    }
}
