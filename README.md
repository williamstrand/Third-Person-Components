# Third person character components
I have created movement and camera components for a third person game.
They are mostly for my portfolio but should be able to be used in a real game. 

## [`CameraComponent`](Runtime/Camera/CameraComponent.cs)
### Usage:
For the [`CameraComponent`](Runtime/Camera/CameraComponent.cs) to work, the main camera need to have the [`CameraBrain`](Runtime/Camera/CameraBrain.cs) script attached to it.
The [`CameraBrain`](Runtime/Camera/CameraBrain.cs) class has a static method `Attach` that takes a [`CameraAttachment`](Runtime/Camera/CameraAttachment.cs) as an argument.
[`CameraAttachment`](Runtime/Camera/CameraAttachment.cs) is a class that has the properties `Position` and `Forward`.
The `Position` is the target position of the camera and the `Forward` is the target forward vector of the camera.
By changing the values of these properties the camera will smoothly update its position and facing direction.

The [`CameraComponent`](Runtime/Camera/CameraComponent.cs) has a public method `Rotate` that takes a `Vector2` and a `float` as arguments.
The `Vector2` is the input from the mouse and the `float` is the sensitivity of the camera.
This method will rotate the camera around the player.

The [`CameraComponent`](Runtime/Camera/CameraComponent.cs) also has multiple public properties that can be used to change the behavior of the camera:
- `RotationLimits` is the limits of the rotation of the camera in the y axis.
- `Target` is the target of the camera.
- `CameraOffset` is the offset from the player to the camera.
- `CameraDistance` is the distance from the player to the camera.

### How it works:
In the `Start` method a new `GameObject`. This is used as a camera boom to handle rotation and positioning of the camera easier.
A new [`CameraAttachment`](Runtime/Camera/CameraAttachment.cs) is also created and the camera is attached to the [`CameraBrain`](Runtime/Camera/CameraBrain.cs) if `attachOnStart` is set to true.
```csharp
void Start()
{
    cameraBoom = new GameObject("Camera Boom").transform;
    cameraBoom.hideFlags = HideFlags.HideInHierarchy;
    
    cameraAttachment = new CameraAttachment();
    if (attachOnStart) CameraBrain.Attach(cameraAttachment);
}
```

When the `Rotate` method is called, the `targetXRotation` and `targetYRotation` are updated.
The `targetXRotation` is then clamped to the x and y values of `RotationLimits`.
```csharp
public void Rotate(Vector2 direction, float speed)
{
    // Update target x and y rotation
    targetXRotation -= direction.y * speed;
    targetYRotation += direction.x * speed;

    // Clamp the rotation of the camera boom to the rotation limits
    targetXRotation = Mathf.Clamp(targetXRotation, rotationLimits.x, rotationLimits.y);
}
```

In the `Update` method the rotation of the camera boom is lerped to the target rotation and the `cameraAttachment.Position` and `cameraAttachment.Forward` is updated accordingly.
```csharp
void Update()
{
    // Lerp the rotation of the camera boom to the target rotation
    var eulerAngles = cameraBoom.eulerAngles;
    var currentXRotation = Mathf.LerpAngle(eulerAngles.x, targetXRotation, cameraSmoothing * Time.deltaTime);
    var currentYRotation = Mathf.LerpAngle(eulerAngles.y, targetYRotation, cameraSmoothing * Time.deltaTime);

    // Set the rotation of the camera boom
    cameraBoom.eulerAngles = new Vector3(currentXRotation, currentYRotation, 0);

    // Set the position of the camera
    cameraAttachment.Position = cameraBoom.position + cameraDistance * -cameraBoom.forward;

    // Set the position of the camera boom
    cameraBoom.position = TargetPosition;

    // Set the rotation of the camera
    var directionToTarget = (TargetPosition - cameraAttachment.Position).normalized;
    cameraAttachment.Forward = directionToTarget;
}
```

## [`CameraBrain`](Runtime/Camera/CameraBrain.cs)
### Usage:
The [`CameraBrain`](Runtime/Camera/CameraBrain.cs) handles the camera position and rotation.
It works by attaching it to a [`CameraAttachment`](Runtime/Camera/CameraAttachment.cs) using the `Attach` method
and then updating the position and rotation of the camera to the position and rotation of the [`CameraAttachment`](Runtime/Camera/CameraAttachment.cs) it is attached to.

### How it works:
The `Attach` method updates the `currentAttachment` of the brain and subscribes to the `OnValueChanged` event of the [`CameraAttachment`](Runtime/Camera/CameraAttachment.cs).
Then it updates the `targetPosition` and `targetForward` of the brain to the `Position` and `Forward` of the [`CameraAttachment`](Runtime/Camera/CameraAttachment.cs).

```csharp
public static void Attach(CameraAttachment attachment)
{
    if (instance.currentAttachment != null)
    {
        instance.currentAttachment.OnValueChanged -= instance.OnAttachmentValueChanged;
    }
    
    instance.currentAttachment = attachment;
    instance.currentAttachment.OnValueChanged += instance.OnAttachmentValueChanged;
    instance.targetForward = attachment.Forward;
    instance.targetPosition = attachment.Position;
}

void OnAttachmentValueChanged(Vector3 position, Vector3 forward)
{
    targetPosition = position;
    targetForward = forward;
}
```

The `Position` and `Forward` properties of the [`CameraAttachment`](Runtime/Camera/CameraAttachment.cs) automatically invokes `OnValueChanged` when they are updated.
```csharp
public Action<Vector3, Vector3> OnValueChanged { get; set; }

public Vector3 Position
{
    get => position;
    set
    {
        position = value;
        OnValueChanged?.Invoke(Position, Forward);
    }
}

public Vector3 Forward
{
    get => forward;
    set
    {
        forward = value;
        OnValueChanged?.Invoke(Position, Forward);
    }
}

Vector3 position;
Vector3 forward;
```

In the `FixedUpdate` method the position and rotation of the camera is lerped to the `targetPosition` and `targetForward` of the brain.
```csharp
void FixedUpdate()
{
    cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, Time.fixedDeltaTime * PositionSmoothTime);
    cameraTransform.forward = Vector3.Lerp(cameraTransform.forward, targetForward, Time.fixedDeltaTime * RotationSmoothTime);
}
```

## [`ThirdPersonMovementComponent`](Runtime/Movement/ThirdPersonMovementComponent.cs)
### Usage:
The [`ThirdPersonMovementComponent`](Runtime/Movement/ThirdPersonMovementComponent.cs) is used to move the character.
It has a public method `Move` that takes a `Vector2` and a `Vector3` as arguments.
The `Vector2` is the movement direction and the `Vector3` is the forward direction of the camera.
This method will move the character in the direction of the movement input.

The `Jump` method is used to make the character jump.

The [`ThirdPersonMovementComponent`](Runtime/Movement/ThirdPersonMovementComponent.cs) also has multiple public properties that can be used to change the behavior of the movement:
- `Speed` is the speed of the character.
- `Acceleration` is the acceleration of the character.
- `RotationSpeed` is the speed of the rotation of the character.
- `AutoRotate` is a bool that determines if the character should rotate towards the movement direction automatically.
- `JumpHeight` is the height of the jump.

### How it works:
The `Move` method takes the movement direction and the forward direction of the camera as arguments and translates the direction to the local space of the camera.
Then it updates the `currentDirection` of the character to the translated direction and the `targetRotation` to the quaternion rotation of the translated direction.
```csharp
public override void Move(Vector2 direction, Vector3 forward)
{
    if (direction.sqrMagnitude == 0) return;
    
    // Translate direction to forward vector
    var right = Vector3.Cross(Vector3.up, forward);
    var direction3 = new Vector3(direction.x, 0, direction.y);
    var translatedDirection = direction3.x * right + direction3.z * forward;
    
    // Set direction and target speed
    currentDirection = new Vector3(translatedDirection.x, 0, translatedDirection.y);
    
    // Set target rotation
    targetRotation = Quaternion.LookRotation(currentDirection.normalized);
}
```

In the `FixedUpdate` method the `currentSpeed` is lerped to the `Speed`. 
The speed is then set to 0 to make the character stop if no input is pressed the next frame.
Then the character is moved in the direction of the `currentDirection` with the speed of `currentSpeed`.
If `autoRotate` is set to true the character is rotated towards the `targetRotation` with the speed of `rotationSpeed`.
```csharp
void FixedUpdate()
{
    // Update speed
    currentSpeed = Mathf.MoveTowards(currentSpeed, Speed, Time.fixedDeltaTime * acceleration);
    Speed = 0;

    // Move character
    var velocity = currentDirection * currentSpeed;
    rigidbody.MovePosition(rigidbody.position + velocity * Time.fixedDeltaTime);

    if (!autoRotate) return;

    // Update rotation
    var lookRotation = Quaternion.Lerp(CurrentRotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
    rigidbody.MoveRotation(lookRotation);
}
```

The `Jump` method sets the `velocity` of the `Rigidbody` to the square root of `JumpHeight` multiplied by `-2` and `gravity`.
```csharp        
public void Jump()
{
    if (!CheckIfGrounded()) return;
    if (rigidbody.velocity.y > 0) return;

    rigidbody.velocity += new Vector3(0, jumpHeight, 0);
}

bool CheckIfGrounded()
{
    var size = Physics.SphereCastNonAlloc(rigidbody.position, groundCheckRadius, Vector3.down, new RaycastHit[1], groundCheckDistance, groundLayer);
    return size > 0;
}
```