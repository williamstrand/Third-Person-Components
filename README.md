# Third person character components
I have created movement and camera components for a third person game.
They are mostly for my portfolio but should be able to be used in a real game. 

## [`CameraComponent`](Runtime/CameraComponent.cs)
For the [`CameraComponent`](Runtime/CameraComponent.cs) to work, the main camera need to have the [`CameraBrain`](Runtime/CameraBrain.cs) script attached to it.
The [`CameraBrain`](Runtime/CameraBrain.cs) class has a static method `Attach` that takes a [`CameraAttachment`](Runtime/CameraAttachment.cs) as an argument.
[`CameraAttachment`](Runtime/CameraAttachment.cs) is a class that has the properties `Position` and `Forward`.
The `Position` is the target position of the camera and the `Forward` is the target forward vector of the camera.
By changing the values of these properties the camera will smoothly update its position and facing direction.

The [`CameraComponent`](Runtime/CameraComponent.cs) has a public method `Rotate` that takes a `Vector2` and a `float` as arguments.
The `Vector2` is the input from the mouse and the `float` is the sensitivity of the camera.
This method will rotate the camera around the player.

The [`CameraComponent`](Runtime/CameraComponent.cs) also has multiple public properties that can be used to change the behavior of the camera:
- `RotationLimits` is the limits of the rotation of the camera in the y axis.
- `Target` is the target of the camera.
- `CameraOffset` is the offset from the player to the camera.
- `CameraDistance` is the distance from the player to the camera.

## [`MovementComponent`](Runtime/MovementComponent.cs)