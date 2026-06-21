# Devlog 1  
  
Recently, and as summer's been coming around, I've been playing more and more Lethal Company (LC) with the bros. Now I wouldn't say we're sweats or anything, but we're pretty good and can get to 3000+ quotas without many resets. On thing that's made that possible is the wounderful Company Cruiser! For anyone who's driven the Company Cruiser (CC) before, you already know ho hellish it can be. The gear changes, janky steering and funny physics make it truly an experiance to manage.   
  
I've been driving this thing for a while now and am quite profficient, but looking around my room got me thinking:
What if I could use my Logitech steering wheel to drive the cruiser? Now of course the base game has no support for it and the input system used to drive the Cruiser is 1-0 based and not Vector based (W key accelerates at max when a pedal would be an axis), but having modded lethal before I was sure It'd be feasable. So I got cracking.  
  
## Configuring the Wheel  

The Logitech G25 is a legacy wheel and is not supported in newer logitech softwares. After much fiddling I figured out I needed to use a very old version of Logitech Gaming Software to get it to register properly on my computer.  
  
## Bringing it into Unity  
  
To start I needed to learn how the wheel interacts in and gets registered by Unity, specifically Unity `2022.3.9f`, the version Zeekerss used to build LC. I began by making a unity project in that version with the URP Template, as it already had a preconfigured controller using the new Input System. I also downloaded a unity project by Juandarn called [SpaceVR](https://github.com/juandarn/SpaceVR-Unity-LogitechG25) that used the logitech G25 to figure out how it configured the wheel in it's input system. Here's what I learnt:
- The G25 is interpreted as a joystick with the wheel being Left and Right (X), Accelerator being Y+, and the Brake being Y-. 
- All the buttons are regular. 
- The Dpad is also a joystick. 
- Z axis is the accelerator pedal but inverted (???)
- RZ axis is the brake pedal but inverted (also ???)
- Trigger is just button 1
- Slider is inverted Clutch pedal

## Looking at Lethal Company's input system

As the goal is to be able to drive the CC with the G25, it was also time to start looking at how Zeekerss configured the Input system for LC. I did this by using an asset ripper to reverse engineer Zeekerss LC build and trun it back into a Unity Project. So now that I have LC's source code, it turns out it's pretty simple and all the controls under one Action Map, so in the end I'll need to add the G25's axis configurations to the Actions themselfs. I don't think it'll be too hard but for that I'll need to start on some reasearch related to interacting with a player's input system through an LC mod patch.

## Next up:
- Programming basic mod for Lethal that adds the wheel's axis to the base game's player input actions.
- Looking into how the Company Cruiser interprets player inputs to see what needs to be reprogrammed. 

This will be quite the fun mod and journey, post a comment if you have any ideas or suggestions!
Cheers,
Liam


# Devlog 2

Finally finished the first and most cruicial part of the mod! The first step to this mod was to let Lethal Company's input system receive bindings from the G25. Now this in theory should have been a fast and easy implementation. Here's how it turned into a 4 hour journey:

## Writing base code

I started off by writing the essential parts of the mod. This mod uses BenInEx and HarmonyLib to patch my custom code into Lethal Company, so I made the public BasicUnityPlugin class that applies all patches in the `.GetExecutingAssembly()`. This means that it'll look at all the code in the built `.dll` and patch automatically instead of me specifying what classes I want to patch.

## The AddWheelBindings class

This is the center of attention when it comes to the input patching. This class needs to do 3 things:

- Find the localPlayerController
- Get all InputActionAssets that affect the localPlayerController
- Add new bindings to the ActionAsset's InputAction that needs it

By fiddling around with the input system in another project, I found the name of the wheel to be `<HID::G25 Racing Wheel>`. Knowing the device name, I was ready to start patching the new inputs into the input assets. 

The class that we will be patching is the StartOfRound class as it handles a lot of the local player and game logic. First I found the local player through `StartOfRound.Instance?.localPlayerController;`, then found the player's playerActions asset and assigned a var to that. Then I made a function that takes the InputActionAsset, finds the actions we want, such as the 'move' action, and applies a new binding to that action. For example, we can grab the 'interact' action with `InputAction interact = actions.FindAction("Interact");` and give it a new binding specifically for the G25: `jump.AddBinding("<HID::G25 Racing Wheel>/button7").WithInteraction("press");`

## Weirdness

Once I finished adding all the bindings to the right actions along with some null checks, I build the `.dll` and got to testing. Right off the bat nothing worked, but a quick change to what method we needed to patch... kind of fixed things?

Something weird was going on: the bindings I'd made to the look action were working, but none of the other were.

3 hour long story short, Zeekerss made his `PlayerControllerB` read from two different instances of one InputActionAsset, so the bindings and actions across the two were the same but each one was being referanced in different spots.

The camera worked right away because the line that read the camera's input was:
`playerActions.Movement.Look.ReadValue<Vector2>()` as opposed to all other controls read as such: `InputSystem.actions.FindAction("Move").ReadValue<Vector2>();`.

The InputActionAsset I was patching was the one created in the player's `Awake()` method, `playerActions`, and not the second global one.
The only reason I found for Zeekerss to make two is that the mouse was being read by the first because the look input is not one that can be rebound in the settings menu. All the controls pulled from the second asset are controls that can be rebound in settings, meaning that the second one was the player's local input settings when the first was the default. 

## Fix

All that needed to be done for the G25 controls to be read for all the actions was to also patch `InputSystem.actions`'s asset. It took about 3 1/2 hours to figure this  out, but at least it works now!

## Next up: 

Going forward we need to: 
- Test the current system on the cruiser
- See what needs changes to feel more realistic

Cheers!
Liam

# Devlog 3

Tried driving the company cruiser with the wheel, and a few things need to be done:
- The CC's steering needs to be turned 1:1 with the wheel's input
- Acceleration and breaking needs to be rewritten to feel more realistic

Published the input part of the mod onto Thunderstore tho to make testing easier.

I'll make a more interesting update once I start working that out!

Cheers,
Liam

# Devlog 4

Finished re-writing the cruiser's steering!
Not too much to say, but still a pretty big update: The cruiser's steering wheel is now 1:1 with the physical G25 in multiple ways.

Originally the Company Cruiser was driven by holding down the A or D keys to steer. The wheel would gradually rotate in the direction you were holding and stop rotating but maintain the rotation on release. This logic obviously sucks when you want to use a sim wheel, because you want the CC's wheel rotation to be a direct replica of the physical wheel's rotation.

This means the logic had to be redone, therefore a rewrite of the cruiser's `GetVehicleInput()` needed to be done. 

## Implementation

Zeekerss original code for steering was this:  
`float num = __instance.steeringWheelTurnSpeed; `  
`__instance.steeringInput = Mathf.Clamp(__instance.steeringInput + __instance.moveInputVector.x * num * Time.deltaTime, -3f, 3f);`

Modifing these lines was pretty straightforward:
- Remove `num` entirely as we done need the wheel's turn speed
- Don't multiply by `Time.deltaTime` as it'll be direct
- Don't add the old input values to the new one as we want 1:1 with the G25

In the end the code looked like this:  
`__instance.steeringInput = Mathf.Clamp(__instance.moveInputVector.x * 3f, -3f, 3f);`

We need to multiply by 3 as we want the max to be 3 but the input can only reach a max value of 1.

## Animation

This new wheel logic worked great except one problem: The G25 would be at 90deg at it's max but the game would show it at 225deg.

The solution to this was to adjust the anim's clam values from +/- 1 to +/- 2.4.  
Why? Because we need to remove 1.4f from the max value, and for some reason the anim needs us to add it onto 1 instead of remove ¯\\\_(ツ)_/¯

## Next Up:

Going forward all that's really left is using the G25's shifter to change the cruiser's gears from Park, Reverse and Drive!

Cheers,  
Liam


# Devlog 5

After I got the steering working, I wanted to be able to use the G25's shifter to change gears in the company cruiser. This required more expirimenting to figure out how the shifter is interpreted by unity's input system.
I turns out that every position on the shifter is assigned a button, and that button is considered pressed when the shifter is in that gear. Below are the button assignements I found:
**In 6 Gear Mode**
- 1st Gear: Button 9
- 2nd Gear: Button 10
- 3rd Gear: Button 11
- 4th Gear: Button 12
- 5th Gear: Button 13
- 6th Gear: Button 14
- Reverse: Button 15
**In +/- Mode**
- Forward: Button 9
- Back: Button 10