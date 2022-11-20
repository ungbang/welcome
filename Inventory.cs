using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    private OrderManager theOrder; //�÷��� Ű �����ִ� ��

    private Inventoryslot[] slots; //�κ��丮 ���Ե�

    private List<Item> inventoryItemList; //�÷��̾ ������ ������ ����Ʈ
    private List<Item> inventoryTabList; // ���õ� �ǿ� ���� �ٸ��� ������ ������ ����Ʈ

    public Text Description_Text; //������ �ο����� Text
    public string[] tabDescription; //�� �ο�����

    public Transform tf; // slot �θ�ü (= Grid Slot)

    public GameObject go; //�κ��丮 Ȱ��/��Ȱ��ȭ
    public GameObject[] selectedTabImages;

    private int selectedItem; //���õ� ������
    private int selectedTab;

    private bool activated; //�κ��丮 Ȱ���� true
    private bool tabActivated; //�� Ȱ��ȭ�� true
    private bool itemActivated; //������ Ȱ��ȭ�� true
    private bool stopKeyInput; //Ű�Է� ���� (�Һ��� �� ���ǰ� ������ �� �׶� �����ϴ� ��)
    private bool preventExec; //�ߺ����� ���� �̰� �Ⱦ��� �����

    private WaitForSeconds waitTime = new WaitForSeconds(0.01f);


    void Start()
    {
        theOrder = FindObjectOfType<OrderManager>();
        inventoryItemList = new List<Item>();
        inventoryTabList = new List<Item>();
        slots = tf.GetComponentsInChildren<InventorySlot>(); //Grid Slot�� �ڽİ�ü���� ���� slots�� ���� ��

    }

    public void RemoveSlot() //�κ��丮 ���� �ʱ�ȭ
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
    } //�� Ȱ��ȭ
    public void SelectedTab()
    {
        StopAllCoroutines();
        Color color = selectedTabImages[selectedTab].GetComponent<Image>().color;  //Tab ���������� ��¦��¦�ϰ� ���ִ� �ڵ�
        color.a = 0f;
        for (int i = 0; i < selectedTabImages.Length; i++)
        {
            selectedTabImages[i].GetComponent<Image>().color = color;
        }
        Description_Text.text = tabDescription[selectedTab]; // ��¦��¦�Ǳ� �� �ڵ�

        StartCoroutine(SelectedTabEffectCoroutine()); //��¦��¦ �����ڵ�
    } //���õ� ���� �����ϰ� �ٸ� ��� ���� �÷� ���İ� 0���� ����
    IEnumerator SelectedTabEffectCoroutine() 
    {
        while (tabActivated)
        {
            Color color = selectedTabImages[0].GetComponent<Image>().color;
            while(color.a < 0.5f ) //�������ϰ� ��
            {
                color.a += 0.03f; //0.03�� ��¦��
                selectedTabImages[selectedTab].GetComponent<Image>().color = color;  // ���õ� �Ǹ� �������ϰ�
                yield return waitTime;
            }

            while (color.a > 0f) //�������� ������� ���ƿ�
            {
                color.a -= 0.03f; 
                selectedTabImages[selectedTab].GetComponent<Image>().color = color;  
                yield return waitTime;
            }

            yield return new WaitForSeconds(0.3f);
        }
    } // ���õ� �� ��¦�� ȿ��


    public void ShowItem()
    {
        inventoryTabList.Clear(); //���� -> �Ҹ�ǰ���� �Ѿ�� â�� ��ġ�� �ȵű� ������
        RemoveSlot();
        selectedItem = 0; //ù��° �������� ������ 0��°�� ��

        switch(selectedTab) //�ǿ� ���� ������ �з�, �װ��� ������ ����Ʈ�� �߰�
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

        for(int i = 0; i < inventoryTabList.Count; i++) //�κ��丮 é ����Ʈ�� ������ �κ��丮 ���Կ� �߰�
        {
            slots[i].gameObject.SetActive(true);
            slots[i].Additem(inventoryTabList[i]);
        }

        SelectedItem();

    } //������ Ȱ��ȭ (inventoryTabList�� ���ǿ� �´� �����۵鸸 �־��ְ�, �κ��丮 ���Կ� ���)
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
            Description_Text = "�ش� Ÿ���� �������� �����ϰ� ���� �ʽ��ϴ�.";
    } // ���õ� �������� �����ϰ�, �ٸ� ��� ���� �÷� ���İ��� 0���� ����
    IEnumerator SelectedItemEffectCoroutine()
    {
        while (itemActivated)
        {
            Color color = slots[0].GetComponent<Image>().color;
            while (color.a < 0.5f) //�������ϰ� ��
            {
                color.a += 0.03f; //0.03�� ��¦��
                slots[selectedItem].selected_Item.GetComponent<Image>().color = color;  // ���õ� �Ǹ� �������ϰ�
                yield return waitTime;
            }

            while (color.a > 0f) //�������� ������� ���ƿ�
            {
                color.a -= 0.03f;
                slots[selectedItem].selected_Item.GetComponent<Image>().color = color;
                yield return waitTime;
            }

            yield return new WaitForSeconds(0.3f);
        }
    } // ���õ� ������ ��¦�� ȿ��


    void Update()
    {
        if(!stopKeyInput)
        {
            if(Input.GetKeyDown(KeyCode.I))
            {
                activated = !activated; // False -> True�� True -> False�� �ٲپ��ִ� ��

                if(activated)
                {
                    
                    theOrder.NotMove();
                    go.SetActive(true); // �κ� â ����
                    selectedTab = 0;
                    tabActivated = true;
                    itemActivated = false;
                    ShowTab();
                    

                }
                else //i�� �� �������� �κ��丮�� ������� �ڵ�
                {
                    
                    StopAllCoroutines();
                    go.SetActive(false);
                    tabActivated = false;
                    itemActivated = false;
                    theOrder.Move(); //�ٽ� ������ �� �ְ� ����

                }
            }

            if(activated)
            {
                if(tabActivated)
                {
                    if(Input.GetKeyDown(KeyCode.RightArrow)) //������ Ű ������ �������� Tab�κ��� �����̴°�
                    {
                        if (selectedTab < selectedTabImages.Length - 1)
                            selectedTab++;
                        else
                            selectedTab = 0;
                        
                        SelectedTab();
                    }
                    else if (Input.GetKeyDown(KeyCode.LeftArrow)) //���� Ű ������ �������� Tab�κ��� �����̴°�
                    {
                        if (selectedTab > 0)
                            selectedTab--;
                        else
                            selectedTab = selectedTabImages.Length - 1; // -1�� �ϴ� ������ 0���� �����̱� ������
                        
                        SelectedTab();
                    }
                    else if (Input.GetKeyDown(KeyCode.Z)) // Tab Ŭ���Ǹ� ������ ������ â
                    {
                       
                        Color color = selectedTabImages[selectedTab].GetComponent<Image>().color;
                        color.a = 0.25f;
                        selectedTabImages[selectedTab].GetComponent<Image>().color = color;
                        itemActivated = true;
                        tabActivated = false;
                        preventExec = true; //�� �ΰ��� Activated�� ���ÿ� ����Ǹ� �ȵǱ� ������
                        ShowItem();
                    }
                } //�� Ȱ��ȭ�� Ű�Է� ó��

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
                            if (selectedTab == 0) //����
                            {
                                
                                stopKeyInput = true;
                                // ���� ȣ��
                            }
                            if (selectedTab == 1) //�Ҹ�ǰ
                            {

                            }
                            else
                            {
                                
                            }
                        }
                    }
                    
                    if (Input.GetKeyCode.X) //������ â���� tab���� �Ű��ִ� ����
                    {
                        
                        StopAllCoroutines();
                        itemActivated = false;
                        tabActivated = true;
                        ShowTab();
                    }
                } //������ Ȱ��ȭ�� Ű�Է� ó��

                if(Input.GetKeyUp(KeyCode.Z)) // �ߺ� ���� ����
                    preventExec = false; 
           }
        }
    }

}
