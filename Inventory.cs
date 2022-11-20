using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    private OrderManager theOrder; //플레이 키 막아주는 것

    private Inventoryslot[] slots; //인벤토리 슬롯들

    private List<Item> inventoryItemList; //플레이어가 소지한 아이템 리스트
    private List<Item> inventoryTabList; // 선택된 탭에 따라 다르게 보여질 아이템 리스트

    public Text Description_Text; //아이템 부연설명 Text
    public string[] tabDescription; //탭 부연설명

    public Transform tf; // slot 부모객체 (= Grid Slot)

    public GameObject go; //인벤토리 활성/비활성화
    public GameObject[] selectedTabImages;

    private int selectedItem; //선택된 아이템
    private int selectedTab;

    private bool activated; //인벤토리 활성시 true
    private bool tabActivated; //탭 활성화시 true
    private bool itemActivated; //아이템 활성화시 true
    private bool stopKeyInput; //키입력 제한 (소비할 때 질의가 나오는 때 그때 방지하는 것)
    private bool preventExec; //중복실행 제한 이거 안쓰면 지우기

    private WaitForSeconds waitTime = new WaitForSeconds(0.01f);


    void Start()
    {
        theOrder = FindObjectOfType<OrderManager>();
        inventoryItemList = new List<Item>();
        inventoryTabList = new List<Item>();
        slots = tf.GetComponentsInChildren<InventorySlot>(); //Grid Slot의 자식개체들이 전부 slots로 들어가게 됨

    }

    public void RemoveSlot() //인벤토리 슬롯 초기화
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].RemoveItem();
            slots[i].gameObject.SetActive(false);
        }
    }

    public void ShowTab()
    {
        RemoveSlot();
        SelectedTab();
    } //탭 활성화
    public void SelectedTab()
    {
        StopAllCoroutines();
        Color color = selectedTabImages[selectedTab].GetComponent<Image>().color;  //Tab 선택했을때 반짝반짝하게 해주는 코드
        color.a = 0f;
        for (int i = 0; i < selectedTabImages.Length; i++)
        {
            selectedTabImages[i].GetComponent<Image>().color = color;
        }
        Description_Text.text = tabDescription[selectedTab]; // 반짝반짝되기 전 코드

        StartCoroutine(SelectedTabEffectCoroutine()); //반짝반짝 실행코드
    } //선택된 탭을 제외하고 다른 모든 탭의 컬러 알파값 0으로 조정
    IEnumerator SelectedTabEffectCoroutine() 
    {
        while (tabActivated)
        {
            Color color = selectedTabImages[0].GetComponent<Image>().color;
            while(color.a < 0.5f ) //반투명하게 됨
            {
                color.a += 0.03f; //0.03초 반짝임
                selectedTabImages[selectedTab].GetComponent<Image>().color = color;  // 선택된 탭만 반투명하게
                yield return waitTime;
            }

            while (color.a > 0f) //반투명이 원래대로 돌아옴
            {
                color.a -= 0.03f; 
                selectedTabImages[selectedTab].GetComponent<Image>().color = color;  
                yield return waitTime;
            }

            yield return new WaitForSeconds(0.3f);
        }
    } // 선택된 탭 반짝임 효과


    public void ShowItem()
    {
        inventoryTabList.Clear(); //열쇠 -> 소모품으로 넘어갈때 창이 겹치면 안돼기 때문에
        RemoveSlot();
        selectedItem = 0; //첫번째 아이템은 무조건 0번째가 됨

        switch(selectedTab) //탭에 따른 아이템 분류, 그것을 아이템 리스트에 추가
        {
            case 0:
                for(int i = 0; i < inventoryItemList.Count; i++)
                {
                    if (Item.ItemType.Key == inventoryItemList[i].itemType)
                        inventoryTabList.Add(inventoryItemList[i]);
                }
                break;
            case 1:
                for (int i = 0; i < inventoryItemList.Count; i++)
                {
                    if (Item.ItemType.Use == inventoryItemList[i].itemType)
                        inventoryTabList.Add(inventoryItemList[i]);
                }
                break;
        }

        for(int i = 0; i < inventoryTabList.Count; i++) //인벤토리 챕 리스트의 내용을 인벤토리 슬롯에 추가
        {
            slots[i].gameObject.SetActive(true);
            slots[i].Additem(inventoryTabList[i]);
        }

        SelectedItem();

    } //아이템 활성화 (inventoryTabList에 조건에 맞는 아이템들만 넣어주고, 인벤토리 슬롯에 출력)
    public void SelectedItem()
    {
        StopAllCoroutines();
        if (inventoryTabList.Count > 0)
        {
            Color color = slots[0].selected_Item.GetComponent<Image>().color;
            color.a = 0f;
            for (int i = 0; i < inventoryTabList.Count; i++)
            {
                slots[i].selected_Item.GetComponent<Image>().color = color;
            }
            Description_Text.text = inventoryTabList[selectedItem].itemDescription;
            StartCoroutine(SelectedItemEffectCoroutine());
        }
        else
            Description_Text = "해당 타입의 아이템을 소유하고 있지 않습니다.";
    } // 선택된 아이템을 제외하고, 다른 모든 탭의 컬러 알파값을 0으로 조정
    IEnumerator SelectedItemEffectCoroutine()
    {
        while (itemActivated)
        {
            Color color = slots[0].GetComponent<Image>().color;
            while (color.a < 0.5f) //반투명하게 됨
            {
                color.a += 0.03f; //0.03초 반짝임
                slots[selectedItem].selected_Item.GetComponent<Image>().color = color;  // 선택된 탭만 반투명하게
                yield return waitTime;
            }

            while (color.a > 0f) //반투명이 원래대로 돌아옴
            {
                color.a -= 0.03f;
                slots[selectedItem].selected_Item.GetComponent<Image>().color = color;
                yield return waitTime;
            }

            yield return new WaitForSeconds(0.3f);
        }
    } // 선택된 아이템 반짝임 효과


    void Update()
    {
        if(!stopKeyInput)
        {
            if(Input.GetKeyDown(KeyCode.I))
            {
                activated = !activated; // False -> True로 True -> False로 바꾸어주는 것

                if(activated)
                {
                    
                    theOrder.NotMove();
                    go.SetActive(true); // 인벤 창 열기
                    selectedTab = 0;
                    tabActivated = true;
                    itemActivated = false;
                    ShowTab();
                    

                }
                else //i를 또 눌렀을때 인벤토리가 사라지는 코드
                {
                    
                    StopAllCoroutines();
                    go.SetActive(false);
                    tabActivated = false;
                    itemActivated = false;
                    theOrder.Move(); //다시 움직일 수 있게 해줌

                }
            }

            if(activated)
            {
                if(tabActivated)
                {
                    if(Input.GetKeyDown(KeyCode.RightArrow)) //오른쪽 키 방향을 눌렀을때 Tab부분이 움직이는것
                    {
                        if (selectedTab < selectedTabImages.Length - 1)
                            selectedTab++;
                        else
                            selectedTab = 0;
                        
                        SelectedTab();
                    }
                    else if (Input.GetKeyDown(KeyCode.LeftArrow)) //왼쪽 키 방향을 눌렀을때 Tab부분이 움직이는것
                    {
                        if (selectedTab > 0)
                            selectedTab--;
                        else
                            selectedTab = selectedTabImages.Length - 1; // -1을 하는 이유는 0부터 시작이기 때문에
                        
                        SelectedTab();
                    }
                    else if (Input.GetKeyDown(KeyCode.Z)) // Tab 클릭되면 나오는 아이템 창
                    {
                       
                        Color color = selectedTabImages[selectedTab].GetComponent<Image>().color;
                        color.a = 0.25f;
                        selectedTabImages[selectedTab].GetComponent<Image>().color = color;
                        itemActivated = true;
                        tabActivated = false;
                        preventExec = true; //위 두개의 Activated가 동시에 실행되면 안되기 때문에
                        ShowItem();
                    }
                } //탭 활성화시 키입력 처리

                else if(itemActivated)
                {
                    if(inventoryTabList.Count > 0 )
                    {
                        if (Input.GetKeyDown(KeyCode.DownArrow))
                        {
                            if (selectedItem < inventoryTabList.Count - 2)
                                selectedItem += 2;
                            else
                                selectedItem %= 2;
                            
                            SelectedItem();
                        }
                        else if (Input.GetKeyDown(KeyCode.UpArrow))
                        {
                            if (selectedItem < 1)
                                selectedItem -= 2;
                            else
                                selectedItem = inventoryTabList.Count - 1 - selectedItem;
                            
                            SelectedItem();
                        }
                        else if (Input.GetKeyDown(KeyCode.RightArrow))
                        {
                            if (selectedItem < inventoryTabList.Count - 1)
                                selectedItem++;
                            else
                                selectedItem = 0;
                            
                            SelectedItem();
                        }
                        else if (Input.GetKeyDown(KeyCode.LeftArrow))
                        {
                            if (selectedItem > 0)
                            {
                                selectedItem--;
                            }
                            else
                                selectedItem = inventoryTabList.Count - 1;
                            
                            SelectedItem();
                        }
                        else if (Input.GetKeyDown(KeyCode.Z) && !preventExec)
                        {
                            if (selectedTab == 0) //열쇠
                            {
                                
                                stopKeyInput = true;
                                // 질의 호출
                            }
                            if (selectedTab == 1) //소모품
                            {

                            }
                            else
                            {
                                
                            }
                        }
                    }
                    
                    if (Input.GetKeyCode.X) //아이템 창에서 tab으로 옮겨주는 역할
                    {
                        
                        StopAllCoroutines();
                        itemActivated = false;
                        tabActivated = true;
                        ShowTab();
                    }
                } //아이템 활성화시 키입력 처리

                if(Input.GetKeyUp(KeyCode.Z)) // 중복 실행 방지
                    preventExec = false; 
           }
        }
    }

}
