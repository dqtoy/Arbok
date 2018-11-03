using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using UnityEngine;

[TypeConverter(typeof(DirectionConverter))]
public abstract class Direction {
    public static Direction Deserialize(byte directionShort) {
        switch (directionShort) {
            case 0:
                return Up.I;
            case 1:
                return Right.I;
            case 2:
                return Down.I;
            case 3:
                return Left.I;
            default:
                throw new Exception("Invalid direction: " + directionShort);
        }
    }

    public abstract byte Serialize();

    public abstract Vector3 GetMoveVector();

    public abstract Quaternion GetHeadRotation();
}

public class DirectionConverter : TypeConverter {
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type source) {
        return source == typeof(string);
    }

    public override object ConvertFrom(ITypeDescriptorContext context,
        CultureInfo culture, object value) {

        var directionString = (string) value;

        return Direction.Deserialize(byte.Parse(directionString));
    }

    public override object ConvertTo(ITypeDescriptorContext context,
        CultureInfo culture,
        object value, Type destinationType) {

        var direction = (Direction) value;

        return direction.Serialize().ToString();
    }
}
