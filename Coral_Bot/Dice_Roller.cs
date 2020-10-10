using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordBot_Test
{
    class Dice_Roller
    {
        private enum RollerState { NumberFirst, Plus, Minus, D, NumberSecond, End };

        public static async Task DiceRoll(string inputString, SocketMessage e)
        {
            Random random = new Random();
            List<int> rolls = new List<int>();
            int result = 0;
            RollerState state = RollerState.NumberFirst;
            string FirstNumber = "";
            string SecondNumber = "";
            RollerState op = RollerState.Plus;
            string[] split;

            split = inputString.Split(' ');
            if (split.Length >= 1)
            {
                inputString = split[0];
            }


            //first state must always be a number so we'll check that now.
            if (!Char.IsNumber(inputString[0]))
            {
                //if it's not a number, return
                await e.Channel.SendMessageAsync("**-Coral does not understand what you want...-**");
                return;
            }
            else
            {
                FirstNumber += inputString[0];
            }

            //go through a state machine, resolving rolls as we go
            for (int i = 1; i < inputString.Length; i++)
            {
                //First Number State
                if (state == RollerState.NumberFirst)
                {
                    //if the next character is a number, stay in same state
                    if (char.IsNumber(inputString[i]))
                    {
                        FirstNumber += inputString[i];
                    }
                    else if (inputString[i] == '+')
                    {
                        //add or subtract current number from result 
                        if (op == RollerState.Plus)
                        {
                            result += Convert.ToInt32(FirstNumber);
                        }
                        else if (op == RollerState.Minus)
                        {
                            result -= Convert.ToInt32(FirstNumber);
                        }

                        state = RollerState.Plus;
                        op = RollerState.Plus;
                    }
                    else if (inputString[i] == '-')
                    {
                        //add or subtract current number from result 
                        if (op == RollerState.Plus)
                        {
                            result += Convert.ToInt32(FirstNumber);
                        }
                        else if (op == RollerState.Minus)
                        {
                            result -= Convert.ToInt32(FirstNumber);
                        }

                        state = RollerState.Minus;
                        op = RollerState.Minus;
                    }
                    else if (inputString[i] == 'd' || inputString[i] == 'D')
                    {
                        state = RollerState.D;
                    }
                    else
                    {
                        await e.Channel.SendMessageAsync("**-Coral does not understand what you want...-**");
                        return;
                    }
                }
                //Plus state
                else if (state == RollerState.Plus)
                {
                    if (char.IsNumber(inputString[i]))
                    {
                        state = RollerState.NumberFirst;
                        op = RollerState.Plus;
                        FirstNumber = "";
                        FirstNumber += inputString[i];
                    }
                    else
                    {
                        await e.Channel.SendMessageAsync("**-Coral does not understand what you want...-**");
                        return;
                    }
                }
                //Minus state
                else if (state == RollerState.Minus)
                {
                    if (char.IsNumber(inputString[i]))
                    {
                        state = RollerState.NumberFirst;
                        op = RollerState.Minus;
                        FirstNumber = "";
                        FirstNumber += inputString[i];
                    }
                    else
                    {
                        await e.Channel.SendMessageAsync("**-Coral does not understand what you want...-**");
                        return;
                    }
                }
                //D State
                else if (state == RollerState.D)
                {
                    if (char.IsNumber(inputString[i]))
                    {
                        state = RollerState.NumberSecond;
                        SecondNumber = "";
                        SecondNumber += inputString[i];
                    }
                    else
                    {
                        await e.Channel.SendMessageAsync("**-Coral does not understand what you want...-**");
                        return;
                    }
                }
                //Second Number State
                else if (state == RollerState.NumberSecond)
                {
                    if (char.IsNumber(inputString[i]))
                    {
                        SecondNumber += inputString[i];
                    }
                    else if (inputString[i] == '-' || inputString[i] == '+')
                    {
                        //resolve roll
                        int rollResult = 0;
                        try
                        {
                            int first = Convert.ToInt32(FirstNumber);
                            int second = Convert.ToInt32(SecondNumber);
                            for (int x = 0; x < first; x++)
                            {
                                int roll = random.Next(second) + 1;
                                rollResult += roll;
                                rolls.Add(roll);
                            }

                            //add or subtract roll
                            if (op == RollerState.Minus)
                            {
                                result -= rollResult;
                            }
                            else if (op == RollerState.Plus)
                            {
                                result += rollResult;
                            }

                            //mark what next operation is
                            if (inputString[i] == '-')
                            {
                                state = RollerState.Minus;
                                op = RollerState.Minus;
                            }
                            else if (inputString[i] == '+')
                            {
                                state = RollerState.Plus;
                                op = RollerState.Plus;
                            }
                        }
                        catch
                        {
                            await e.Channel.SendMessageAsync("**-Coral drops the dice all over the floor-**");
                            return;
                        }
                    }
                    else
                    {
                        await e.Channel.SendMessageAsync("**-Coral does not understand what you want...-**");
                        return;
                    }
                }
            }

            if (state == RollerState.NumberFirst)
            {
                //add or subtract roll
                if (op == RollerState.Minus)
                {
                    result -= Convert.ToInt32(FirstNumber);
                }
                else if (op == RollerState.Plus)
                {
                    result += Convert.ToInt32(FirstNumber);
                }
            }
            else if (state == RollerState.NumberSecond)
            {
                //resolve roll
                int rollResult = 0;
                try
                {
                    int first = Convert.ToInt32(FirstNumber);
                    int second = Convert.ToInt32(SecondNumber);


                    for (int x = 0; x < first; x++)
                    {
                        int roll = random.Next(second) + 1;
                        rollResult += roll;
                        rolls.Add(roll);
                    }

                    //add or subtract roll
                    if (op == RollerState.Minus)
                    {
                        result -= rollResult;
                    }
                    else if (op == RollerState.Plus)
                    {
                        result += rollResult;
                    }
                }
                catch
                {
                    await e.Channel.SendMessageAsync("**-Coral drops the dice all over the floor-**");
                    return;
                }
            }
            else
            {
                await e.Channel.SendMessageAsync("**-Coral does not understoond what you want...-**");
                return;
            }


            string rollString = "`rolls: ";
            int counter = 0;
            foreach (int r in rolls)
            {
                counter++;
                if (counter > 200) // don't show more than this number of dice rolls
                {
                    rollString += "...";
                    break;
                } // don't show more than this number of dice rolls
                rollString += r.ToString() + ", ";
            }
            rollString = rollString.Trim(new char[] { ',', ' ' });
            rollString += "`";

            await e.Channel.SendMessageAsync("You rolled: " + result + "\n" + rollString);
        }


    }
}
