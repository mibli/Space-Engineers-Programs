IMyPistonBase extender;
IMyPistonBase erector;
IMyMotorStator rotor;
float pistonVelocity;

bool firstOut = true;
IMyTextSurface surface;

public void EchoLCD(string text) // Put this at the top of the script
{
    if (firstOut) {
        if (((IMyTextSurfaceProvider)Me).SurfaceCount > 0)
        {
            surface = ((IMyTextSurfaceProvider)Me)?.GetSurface(0);
        }
        else if (((IMyTextSurfaceProvider)GridTerminalSystem.GetBlockWithName("Drill LCD"))?.SurfaceCount > 0) {
            surface = ((IMyTextSurfaceProvider)Me).GetSurface(0);
        }
        if (surface != null) {
            surface.ContentType = ContentType.TEXT_AND_IMAGE;
            surface.WriteText("", false);
        }
        firstOut = false;
    }
    Echo(text);
    surface?.WriteText(text + Environment.NewLine, false);
}

public Program() {
    pistonVelocity = 0.5F;
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
    Storage = "reset";
}

public void Save() {
}

public void Main(string argument) {
    if (argument == "reset") {
        Storage = "reset";
        return;
    }

    string[] storagev = Storage.Split(';');
    string state;
    if (storagev.Length > 0) {
        state = storagev[0];
    }
    else {
        state = "unknown";
    }

    if (state == "reset") {
        extender = (IMyPistonBase) GridTerminalSystem.GetBlockWithName("Drill Extender");
        erector = (IMyPistonBase) GridTerminalSystem.GetBlockWithName("Drill Erector");
        rotor = (IMyMotorStator) GridTerminalSystem.GetBlockWithName("Drill Rotor");
        if (extender == null) {
            EchoLCD("Couldn't find 'Drill Extender'");
            return;
        }
        if (erector == null) {
            EchoLCD("Couldn't find 'Drill Erector'");
            return;
        }
        if (rotor == null) {
            EchoLCD("Couldn't find 'Drill Rotor'");
            return;
        }
        Storage = "unknown";
    } else if (state == "rotating") {
        if ((rotor.TargetVelocityRad > 0 && rotor.Angle >= rotor.UpperLimitRad) ||
            (rotor.TargetVelocityRad < 0 && rotor.Angle <= rotor.LowerLimitRad)) {
            // finished rotating

            if (rotor.Angle <= rotor.LowerLimitRad && extender.CurrentPosition >= extender.MaxLimit) {
                if (erector.CurrentPosition >= erector.MaxLimit) {
                    // reached max piston reach
                    Storage = "maxreach";
                } else {
                    // finished layer
                    float erectStep = (erector.MaxLimit - erector.MinLimit) / 4;
                    float targetPosition = Math.Min(erector.CurrentPosition + erectStep, erector.MaxLimit);
                    erector.Velocity = pistonVelocity;
                    Storage = "erecting;" + targetPosition;
                }
            } else {
                // prepare for another turn
                float extendStep = (extender.MaxLimit - extender.MinLimit) / 4;
                float targetPosition = Math.Min(extender.CurrentPosition + extendStep, extender.MaxLimit);
                extender.Velocity = pistonVelocity;
                Storage = "extending;" + targetPosition;
            }

        }
    } else if (state == "extending") {
        float targetPosition = 0;
        float.TryParse(storagev[1], out targetPosition);
        if (extender.CurrentPosition >= targetPosition) {
            // finished extending (may overshoot)
            extender.Velocity = 0;
            rotor.TargetVelocityRad = -rotor.TargetVelocityRad;
            Storage = "rotating";
        }
    } else if (state == "erecting") {
        float targetPosition = 0;
        float.TryParse(storagev[1], out targetPosition);
        if (erector.CurrentPosition >= targetPosition) {
            // finished erecting (may overshoot)
            erector.Velocity = 0;
            extender.Velocity = -pistonVelocity;
            Storage = "resetting";
        }
    } else if (state == "resetting") {
        if (rotor.Angle <= rotor.LowerLimitRad && extender.CurrentPosition <= extender.MinLimit) {
            // in layer start position
            extender.Velocity = 0;
            rotor.TargetVelocityRad = -rotor.TargetVelocityRad;
            Storage = "rotating";
        }
    } else if (state == "maxreach") {
        // someone may have forgotten to add erector
        IMyPistonBase oldErector = erector;
        erector = (IMyPistonBase) GridTerminalSystem.GetBlockWithName("Drill Erector");
        if (erector == null) {
            EchoLCD("Couldn't find 'Drill Erector'");
            return;
        } else if (erector != oldErector) {
            if (rotor.TargetVelocityRad > 0) {
                rotor.TargetVelocityRad = -rotor.TargetVelocityRad;
            }
            extender.Velocity = -pistonVelocity;
            Storage = "resetting";
        }
    } else {
        // state unknown, bring to start position
        if (rotor.TargetVelocityRad > 0) {
            rotor.TargetVelocityRad = -rotor.TargetVelocityRad;
        }
        extender.Velocity = -pistonVelocity;
        Storage = "resetting";
    }

    EchoLCD(String.Format("Current state: {0}\n" +
                          "Rotor: {1:F2}, {2:F2} R/s\n" +
                          "Extender: {3:F2}, {4:F2} m/s\n" +
                          "Erector: {5:F2}, {6:F2} m/s",
                          Storage,
                          rotor.Angle, rotor.TargetVelocityRad,
                          extender.CurrentPosition, extender.Velocity,
                          erector.CurrentPosition, erector.Velocity));
}
