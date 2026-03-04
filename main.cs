using Rage;
using System;
using System.Windows.Forms;

[assembly: Rage.Attributes.Plugin("SimpleK9", Description = "Basic K9 Plugin", Author = "Your Name")]

namespace SimpleK9
{
    public class Main : Plugin
    {
        private static Ped k9;
        private static Ped markedSuspect;
        private static bool isFollowing = false;
        private static bool isSitting = false;
        private static bool retired = false;

        public override void Initialize()
        {
            Game.DisplayNotification("~b~K9 Plugin Loaded");
            GameFiber.StartNew(MainLoop);
        }

        private void MainLoop()
        {
            while (true)
            {
                GameFiber.Yield();

                // Open Menu
                if (Game.IsKeyDown(Keys.Multiply)) // Num*
                {
                    OpenMenu();
                    GameFiber.Sleep(300);
                }

                // Mark suspect with U
                if (Game.IsKeyDown(Keys.U))
                {
                    markedSuspect = GetClosestPed();
                    if (markedSuspect)
                        Game.DisplayNotification("~r~Suspect Marked");
                }

                // Follow logic
                if (k9 && k9.Exists() && isFollowing && !retired)
                {
                    k9.Tasks.FollowToOffsetFromEntity(Game.LocalPlayer.Character, new Vector3(0f, -1.5f, 0f), -1);
                }
            }
        }

        private void OpenMenu()
        {
            Game.DisplaySubtitle("1=Spawn | 2=Follow | 3=Sit | 4=Bite | 5=Vehicle | 6=Retire");

            while (true)
            {
                GameFiber.Yield();

                if (Game.IsKeyDown(Keys.D1)) { SpawnK9(); return; }
                if (Game.IsKeyDown(Keys.D2)) { Follow(); return; }
                if (Game.IsKeyDown(Keys.D3)) { Sit(); return; }
                if (Game.IsKeyDown(Keys.D4)) { Bite(); return; }
                if (Game.IsKeyDown(Keys.D5)) { ToggleVehicle(); return; }
                if (Game.IsKeyDown(Keys.D6)) { Retire(); return; }

                if (Game.IsKeyDown(Keys.Back))
                    return;
            }
        }

        private void SpawnK9()
        {
            if (k9 && k9.Exists())
            {
                Game.DisplayNotification("~r~K9 Already Spawned");
                return;
            }

            Model model = new Model("a_c_shepherd");
            model.LoadAndWait();

            Vector3 spawnPos = Game.LocalPlayer.Character.GetOffsetPositionFront(2f);
            k9 = new Ped(model, spawnPos, 0f);

            k9.IsPersistent = true;
            k9.BlockPermanentEvents = true;

            Game.DisplayNotification("~g~K9 Spawned");
        }

        private void Follow()
        {
            if (!k9) return;
            isFollowing = true;
            isSitting = false;
            retired = false;

            Game.DisplayNotification("~b~K9 Following");
        }

        private void Sit()
        {
            if (!k9) return;

            isFollowing = false;
            isSitting = true;

            k9.Tasks.Clear();
            k9.Tasks.PlayAnimation("creatures@rottweiler@amb@world_dog_sitting@idle_a", "idle_b", 1f, AnimationFlags.Loop);

            Game.DisplayNotification("~b~K9 Sitting");
        }

        private void Bite()
        {
            if (!k9 || !markedSuspect) return;
            if (retired) return;

            k9.Tasks.FightAgainst(markedSuspect);

            Game.DisplayNotification("~r~K9 Engaging Suspect");
        }

        private void ToggleVehicle()
        {
            if (!k9) return;

            Vehicle playerVehicle = Game.LocalPlayer.Character.CurrentVehicle;

            if (playerVehicle)
            {
                k9.WarpIntoVehicle(playerVehicle, -2);
                Game.DisplayNotification("~b~K9 Entered Vehicle");
            }
            else
            {
                k9.Tasks.LeaveVehicle(LeaveVehicleFlags.None);
                Game.DisplayNotification("~b~K9 Exited Vehicle");
            }
        }

        private void Retire()
        {
            retired = true;
            isFollowing = false;

            k9.Tasks.Clear();
            Game.DisplayNotification("~r~K9 Retired");
        }

        private Ped GetClosestPed()
        {
            Ped[] peds = World.GetAllPeds();
            Ped closest = null;
            float minDist = 10f;

            foreach (Ped ped in peds)
            {
                if (ped != Game.LocalPlayer.Character && ped.IsAlive)
                {
                    float dist = ped.DistanceTo(Game.LocalPlayer.Character);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = ped;
                    }
                }
            }

            return closest;
        }

        public override void Finally()
        {
            if (k9 && k9.Exists())
                k9.Delete();
        }
    }
}
