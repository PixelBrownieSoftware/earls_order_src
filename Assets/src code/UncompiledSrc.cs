/*
OLD ATTACKING ROUTINE
06/10/2018

 
                            CheckCharacterSurroundings(false);
                            if (targets == null)
                                STATES = STATEMACHINE.SELECT_CHAR;
                            List<o_character> cha = targets.FindAll(x => x.health > 0);

                            if (cha.Count == 0)
                                STATES = STATEMACHINE.SELECT_CHAR;

                            if (cha.Count > 1)
                            {
                                if (PushButton(new Rect(120, 100, 25, 40), "+"))
                                    selectChar++;

                                if (PushButton(new Rect(180, 100, 25, 40), "-"))
                                    selectChar--;

                                selectChar = Mathf.Clamp(selectChar, 0, cha.Count - 1);
                            }
                            else
                                selectChar = 0;

                            for (int i = 0; i < cha.Count - 1; i++)
                            {
                                if (selectChar == i)
                                    cha[i].renderer.color = Color.yellow;
                                else
                                    cha[i].renderer.color = Color.white;
                            }
                            if (selectChar <= cha.Count - 1)
                            {
                                if (cha[selectChar].health > 0 && player.actionPoints >= 2)
                                {
                                    if (PushButton(new Rect(110, 50, 90, 30), "Confirm"))
                                    {
                                        targets[selectChar].renderer.color = Color.white;
                                        STATES = STATEMACHINE.ATTACK_EXECUTE;
                                    }
                                }
                            }
                            if (PushButton(new Rect(110, 160, 90, 30), "Cancel"))
                            {
                                targets[selectChar].renderer.color = Color.white;
                                STATES = STATEMACHINE.SELECT_CHAR;
                            }

 
SKIP OPTION

        if (TURNSTATE == TURNS.ENEMY)
        {
            if (targets.Count > 0)
            {
                if (!IsSkipped)
                {
                    if (PushButton(new Rect(90, 90, 90, 30), "Skip"))
                    {
                        //player.StopAllCoroutines();
                        //s_camera.staticCam.StopAllCoroutines();
                        
                        s_camera.staticCam.StartCoroutine(s_camera.staticCam.Fade(Color.black, 4));
                        IsSkipped = true;
                    }
                }
            }
        }
 

SELECTING TARGET
09/10/2018

                            targetToAttack = targets[selectChar];
                            if (player.actionPoints >= 2)
                                actionpoints = "AP: " + player.actionPoints + " - " + 2 + " = <color=#00ffffff>" + (player.actionPoints - 2) + "</color>" + "\n"
                                    + targetToAttack.name + "'s HP: " + targetToAttack.health +  " to " + "<color=#00ffffff>"+ (targetToAttack.health - player.attack) + "</color>";
                            else
                                actionpoints = player.actionPoints + " - " + 2 + " = <color=#ff0000ff>" + (player.actionPoints - 2) + "</color>";




List<o_character> cha = targets.FindAll(x => x.health > 0);
                                if (cha.Count > 0 && player.actionPoints >= 2)
                                {
                                    if (PushButton(new Rect(90, 190, 90, 30), "Attack"))
                                    {
                                        STATES = STATEMACHINE.ATTACK_SELECT;
                                    }
                                }



Calculating move cost

if (player.actionPoints >= pathfcost)
                                actionpoints = "AP: " + player.actionPoints + " - " + pathfcost + " = <color=#00ffffff>" + (player.actionPoints - pathfcost) + "</color>";
                            else
                                actionpoints = player.actionPoints + " - " + pathfcost + " = <color=#ff0000ff>" + (player.actionPoints - pathfcost) + "</color>";
                            


 */