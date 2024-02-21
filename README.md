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
If a wall is detected between the camera and the target, the distance is set to the distance between the wall and the target.
```csharp
void Update()
{
    var directionToTarget = (TargetPosition - cameraAttachment.Position).normalized;

    // Lerp the rotation of the camera boom to the target rotation
    var eulerAngles = cameraBoom.eulerAngles;
    var currentXRotation = Mathf.LerpAngle(eulerAngles.x, targetXRotation, cameraSmoothing * Time.deltaTime);
    var currentYRotation = Mathf.LerpAngle(eulerAngles.y, targetYRotation, cameraSmoothing * Time.deltaTime);

    // Set the rotation of the camera boom
    cameraBoom.eulerAngles = new Vector3(currentXRotation, currentYRotation, 0);

    // Set the position of the camera
    var distance = cameraDistance;
    if (CheckForWall(out var hitInfo))
    {
        var point = hitInfo.point.Flatten();
        distance = Vector3.Distance(point, target.position.Flatten());
        //distance = hitInfo.distance - 0.1f;
    }

    cameraAttachment.Position = cameraBoom.position + distance * -cameraBoom.forward;

    // Set the position of the camera boom
    // cameraBoom.position = TargetPosition;
    cameraBoom.position = target.position;

    // Set the rotation of the camera
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
- `Acceleration` is the acceleration of the character.
- `RotationSpeed` is the speed of the rotation of the character.
- `AutoRotate` is a bool that determines if the character should rotate towards the movement direction automatically.
- `JumpHeight` is the height of the jump.

### How it works:
The `Move` method takes the movement direction and the forward direction of the camera as arguments and translates the direction to the local space of the camera.
Then it updates the `targetVelocity` of the character to the translated direction multiplied by the speed and the `targetRotation` to the quaternion rotation of the translated direction.
```csharp
public override void Move(Vector2 direction, Vector3 forward, float speed)
{
    if (direction.sqrMagnitude == 0) return;

    // Translate direction to forward vector
    forward.y = 0;
    var right = Vector3.Cross(Vector3.up, forward);
    var direction3 = new Vector3(direction.x, 0, direction.y);
    var translatedDirection = direction3.x * right + direction3.z * forward;

    // Set direction and target speed
    targetVelocity = translatedDirection * speed;

    // Set target rotation
    var velocity = targetVelocity.normalized;
    velocity.y = 0;
    targetRotation = Quaternion.LookRotation(velocity);
}
```

In the `FixedUpdate` method the velocity of the `rigidbody` is moved towards the `targetVelocity`. 
The `targetVelocity` is then set to `Vector3.zero` to make the character stop if no input is pressed the next frame.
If `autoRotate` is set to true the character is rotated towards the `targetRotation` with the speed of `rotationSpeed`.
```csharp
void FixedUpdate()
{
    // Update velocity
    targetVelocity.y = rigidbody.velocity.y;
    rigidbody.velocity = Vector3.MoveTowards(rigidbody.velocity, targetVelocity, Time.fixedDeltaTime * acceleration);
    targetVelocity = Vector3.zero;

    if (!autoRotate) return;

    // Update rotation
    var lookRotation = Quaternion.Lerp(CurrentRotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
    rigidbody.MoveRotation(lookRotation);
}
```

The `Jump` method sets the upwards `velocity` of the `Rigidbody` to `jumpHeight` if the character is grounded and not already jumping.
The `CheckIfGrounded` method uses `Physics.SphereCastNonAlloc` to check if the character is grounded.
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

## [`LedgeGrabComponent`](Runtime/Movement/LedgeGrab/LedgeGrabComponent.cs)
### Usage:
The [`LedgeGrabComponent`](Runtime/Movement/LedgeGrab/LedgeGrabComponent.cs) is used to make the character be able to grab ledges.
It constantly checks if the character is able to grab a ledge and if it is, it will make the character grab the ledge.

It has two public methods `Move` and `Release`. `Move` takes a `Vector2`, `Vector3` and a `float` as arguments.
`Move` makes the character climb on the edge if able to.
The `Vector2` is the movement direction, the `Vector3` is the forward direction of the camera and the `float` is the speed of the movement.
`Release` makes the character release the ledge.

The `OnLedgeGrab` event is invoked when the character grabs a ledge and the `OnLedgeRelease` event is invoked when the character releases the ledge.

The [`LedgeGrabComponent`](Runtime/Movement/LedgeGrab/LedgeGrabComponent.cs) also has multiple variables that can be used to change the behavior of the ledge grab:
- `grabRange` is the distance the character checks for and can grab onto ledges.
- `grabGracePeriod` is how long it takes before the character can grab a ledge again after releasing one.
- `moveDelay` is how long it takes before the character can move again after moving on a ledge.
- `moveDistance` is the distance the character moves when moving on a ledge.

### How it works:
In the `Update` method the `moveDelayTimer` and `grabGraceTimer` are updated. It also checks if the character is able to grab a ledge and if it is, it will make the character grab the ledge.
```csharp
void Update()
{
    // If the character is not moving and the move delay is enabled, decrease the move delay timer
    if (!IsMoving && MoveDelayEnabled)
    {
        moveDelayTimer -= Time.deltaTime;
    }

    // If the grab grace is enabled, decrease the grab grace timer and return
    if (GrabGraceEnabled)
    {
        grabGraceTimer -= Time.deltaTime;
        return;
    }

    // If the character is not grabbing the ledge, check for a ledge
    if (IsGrabbingLedge) return;
    if (!CheckForLedge(out var ledge)) return;

    // Attach to the found ledge
    OnLedgeGrab?.Invoke();
    AttachToLedge(ledge);
}

bool CheckForLedge(out RaycastHit hitInfo, Vector3 origin, Vector3 direction)
{
    return Physics.Raycast(origin, direction, out hitInfo, grabRange, grabbableLayers);
}
```
In the `AttachToLedge` method `targetPosition` and `targetDirection` is updated and the gravity is disabled.
```csharp
void AttachToLedge(RaycastHit ledge)
{
    // Disable gravity and reset velocity
    rigidbody.useGravity = false;
    rigidbody.velocity = Vector3.zero;

    IsGrabbingLedge = true;
    moveDelayTimer = moveDelay;

    // Set target position and direction
    targetPosition = ledge.point + ledge.normal * grabRange / 2;
    targetPosition.y = ledge.transform.position.y;
    targetDirection = -ledge.normal;
}
```
In the `Move` method it first checks if the character is able to move and if it is, it will make the character climb on the edge.
First it translates the movement direction to the local space of the camera.
Then it checks for a ledge in the direction of movement and if it finds one, it will make the character climb on the ledge.
If no ledge is found, it will check for a ledge in the direction of movement, but closer to the character.
If no ledge is still not found, the method will return.
```csharp
public override void Move(Vector2 direction, Vector3 forward, float speed)
{
    if (!IsGrabbingLedge) return;
    if (direction.sqrMagnitude == 0) return;
    if (IsMoving) return;
    if (MoveDelayEnabled) return;
    if (Mathf.Abs(direction.x) < XMoveThreshold) return;

    // Translate direction to camera local space
    var right = Vector3.Cross(Vector3.up, forward);
    var translatedDirection = direction.x * right + direction.y * forward;

    // Check for a ledge in the direction of movement
    var moveDirection = characterTransform.right * (Mathf.Sign(translatedDirection.x) * moveDistance);
    if (!CheckForLedge(out var hitInfo, characterTransform.position + moveDirection, characterTransform.forward))
    {
        // Check for a ledge in the direction of movement, but closer to the character
        if (!CheckForLedge(out hitInfo, characterTransform.position + moveDirection / 2, characterTransform.forward)) return;
    }
    
    currentSpeed = speed;
    AttachToLedge(hitInfo);
}
```
In the `FixedUpdate` method the character is moved towards the `targetPosition` if the character is currently moving on a ledge.
```csharp
void FixedUpdate()
{
    if (!IsGrabbingLedge) return;
    if (!IsMoving) return;

    // Update character position and rotation
    characterTransform.position = Vector3.MoveTowards(characterTransform.position, targetPosition, Time.fixedDeltaTime * currentSpeed);
    characterTransform.forward = Vector3.MoveTowards(characterTransform.forward, targetDirection, Time.fixedDeltaTime * currentSpeed);
}
```