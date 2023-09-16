using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleCardDrawAndSpread_CardDrag;


namespace SimpleCardDrawAndSpread_HandCard
{
    public class HandCardSystem : MonoBehaviour
    {
        //CardDrawSystem script recall settings.
        CardDrawSystem _CardDrawSystem;

        [Header("Card Information")]
        public int id;
        public string name;
        public int score;
        public int turn;
        public string portion;
        public string skill;

        [Header("Card Image")]
        public SpriteRenderer CardIcon_Sprite;
        public SpriteRenderer[] CardLayers;

        [Header("Card Drag")]
        public bool CardUseLock;
        int HandCardNumber = 0; //Use to store card unique numbers.
        Vector3 MouseClick_Pos;

        //Prevent script dispatch when cards are moved automatically
        [Header("Card Draw Move")]
        public bool FirstDrawTrigger;
        public bool HandSpreadTrigger;

        // Start is called before the first frame update
        void Start()
        {
            //CardDrawSystem script recall settings.
            _CardDrawSystem = FindObjectOfType<CardDrawSystem>();
        }

        // Update is called once per frame
        void Update()
        {
            //Set the automatic movement that occurs the first time this card is created and the automatic movement for alignment so that it does not conflict with each other.
            if (FirstDrawTrigger == true)
            {
                AutoMove_FirstDraw_Manager();
            }
            else if (HandSpreadTrigger == true)
            {
                //Recall new whenever the card unique number changes. This number is used to re-align each time a card in your hand is used and destroyed.
                for (int i = 0; i < _CardDrawSystem.PlayerHandCardList.Count; i++)
                {
                    if (this.gameObject == _CardDrawSystem.PlayerHandCardList[i])
                    {
                        HandCardNumber = i;
                    }
                }

                AutoMove_SpreadMove_Manager();
            }

        }


        void AutoMove_FirstDraw_Manager()
        {
            //Automatically moves to the CardHandPos position.
            this.transform.position = Vector2.MoveTowards(this.gameObject.transform.position, _CardDrawSystem.CardHandPos.position, (_CardDrawSystem.CardSpeed_Draw * Time.deltaTime));


            if (Vector2.Distance(this.gameObject.transform.position, _CardDrawSystem.CardHandPos.position) == 0)
            {
                //Change Trigger
                FirstDrawTrigger = false;
                CardUseLock = false;

                //Now that you have drawn a new card, call AutoMove_SpreadMove_Manager() to rearrange the cards in your hand, including those cards.
                for (int i = 0; i < _CardDrawSystem.PlayerHandCardList.Count; i++)
                {
                    HandCardSystem new_HandCardSystem = _CardDrawSystem.PlayerHandCardList[i].GetComponent<HandCardSystem>();

                    new_HandCardSystem.HandSpreadTrigger = true;
                }


            }
        }

        void AutoMove_SpreadMove_Manager()
        {
            //Locate the location stored in the saved HandCardNumber number and move it automatically. Use Lerp to move faster if the card is far away.
            this.transform.position = Vector2.Lerp(this.gameObject.transform.position, _CardDrawSystem.HandCard_EachCardDistanceList[HandCardNumber], (_CardDrawSystem.CardSpeed_HandSpread * Time.deltaTime));

            //Adjust the angle when moved close to the specified position.
            if (Vector2.Distance(this.gameObject.transform.position, _CardDrawSystem.HandCard_EachCardDistanceList[HandCardNumber]) <= 0.05f)
            {
                this.transform.rotation = Quaternion.Euler(0, 0, _CardDrawSystem.HandCard_EachCardAngleList[HandCardNumber]);

                //End automatic movement when position and angle are adjusted.
                HandSpreadTrigger = false;
            }
        }

        //For mouse input, it works when CardUseLock is false. CardUseLock is usually used as true when automatic movement or when it is not your turn.
        private void OnMouseDown()
        {
            if (CardUseLock == false)
            {
                //Save the mouse position you clicked on and exit the auto-alignment of the clicked cards you clicked on.

                if (_CardDrawSystem.CardDragType == CardDragType.CardPos)//When you click a card, it moves from the unique coordinates of the card.
                {
                    MouseClick_Pos = this.gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));

                }
                else if (_CardDrawSystem.CardDragType == CardDragType.MousePos)//Move from mouse coordinates when you click the card.
                {
                    MouseClick_Pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));

                }


                HandSpreadTrigger = false;
            }



        }

        private void OnMouseDrag()
        {
            if (CardUseLock == false)
            {
                //Initializes the angle set in the alignment.
                this.transform.rotation = Quaternion.Euler(0, 0, 0);

                //Move the dragged card object to the mouse position.

                if (_CardDrawSystem.CardDragType == CardDragType.CardPos)
                {
                    Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z);
                    Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition) + MouseClick_Pos;

                    this.transform.position = objPosition;
                }
                else if (_CardDrawSystem.CardDragType == CardDragType.MousePos)
                {
                    Vector3 input_DragPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));

                }

            }


        }

        private void OnMouseUp()
        {
            if (CardUseLock == false)
            {
                if (Vector2.Distance(this.transform.position, _CardDrawSystem.CardUseGround.position) < _CardDrawSystem.CardUseDistance)
                {
                    Debug.Log("OnMoseUp");
                    
                    HandCardSystem inputHandCardSystem = _CardDrawSystem.PlayerHandCardList[HandCardNumber].GetComponent<HandCardSystem>();
                    
                    //第一回合只能丟咒語卡
                    if(_CardDrawSystem.round == 0 && inputHandCardSystem.portion == "咒語"){      //記得改if(_CardDrawSystem.round == 0 && inputHandCardSystem.portion != "咒語")
                        Debug.Log("第一回合只能丟咒語卡");
                    }
                    else{ 
                        Debug.Log("不是第一回合或者是咒語卡");
                        //將準備移除的卡片加入移除卡片的List中
                        _CardDrawSystem.RemoveCardList.Add(inputHandCardSystem);

                        //Remove the used cards from the list and re-align them with the layers of the cards in your hand.
                        _CardDrawSystem.PlayerHandCardList.RemoveAt(HandCardNumber);
                        _CardDrawSystem.RoundCards[_CardDrawSystem.round] = _CardDrawSystem.RoundCards[_CardDrawSystem.round] - 1;
                        Debug.Log("RoundCards[" + _CardDrawSystem.round + "]: " + _CardDrawSystem.RoundCards[_CardDrawSystem.round]);
                    
                        //如果該回合卡片丟完了，將所以丟出去卡片的生效回合(turn)減1，並將回合+1及抽卡，進入下一回合
                        if (_CardDrawSystem.RoundCards[_CardDrawSystem.round] <= 0) {
                            Debug.Log("Round end!!");
                            _CardDrawSystem.CardDelete = true;
                            _CardDrawSystem.countTurn();             
                            _CardDrawSystem.round = _CardDrawSystem.round + 1;                                                       
                            _CardDrawSystem.Button_CardDraw_Manager(); 
                        }
                        //_CardDrawSystem.CardLayerCheckManager();
                        //_CardDrawSystem.CardSpreadSettingManager();
                         
                        //When the numerical alignment is complete, use automatic movement to move the card in your hand to that position.
                        for (int i = 0; i < _CardDrawSystem.PlayerHandCardList.Count; i++)
                        {
                            _CardDrawSystem.PlayerHandCardList[i].GetComponent<HandCardSystem>().HandSpreadTrigger = true;
                        }

                        //Destroy used card.
                        Destroy(this.gameObject);
                    }
                }
                else
                {
                    //Return to original position.
                    HandSpreadTrigger = true;

                }

            }



        }
    }


}
