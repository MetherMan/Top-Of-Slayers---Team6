using Firebase;
using Firebase.Auth;
using Firebase.Extensions; //ContinueWithOnMainThread 사용을 위해 필요
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

public class FirebaseManager : Singleton<FirebaseManager>
{
    #region field
    FirebaseApp app;
    FirebaseAuth auth;
    FirebaseUser user;

    //인증상태 변경
    string displayName;
    string emailAddress;
    string uid;
    public string UID
    {
        get { return uid; }
        private set
        {
            uid = value;
        }
    }
    //System.Uri photoUrl;

    //에러로그 처리
    public System.Action<string, bool> inputFailedEvent;
    public System.Action<string, bool> createFailedEvent;

    FirebaseFirestore db;

    //유저 게임 데이터 (세이브용)
    int userLevel;
    int userCurrentExp;
    List<object> userInventory = new List<object>();
    Dictionary<string, object> userEquipment = new Dictionary<string, object>();
    int userGold;
    int userEnerge;

    //로그인 시 : User.userId keyValue / iventory, equipment 데이터 가져오기

    //아이템 획득 : iventory 리스트 추가
    //아이템 제거 : iventory 리스트 수정/제거 4개중 3개만 팔고 1개 남았을 경우

    //장비 착용 : equipment 리스트 추가
    //장비 해제 : equipment 리스트 제거

    //골드 획득, 사용 : user[2] int 값 수정

    //스테이지 클리어 : 아이템 획득, 골드 획득
    //가챠 : 아이템 획득
    //인벤토리 : 장비 착용, 아이템 제거
    //상점 : 골드 사용 - 터치를 여러 번 할 수 있다.
    #endregion

    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    #region method
    private void Init()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            DependencyStatus dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                //초기화 성공
                app = FirebaseApp.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;
                db = FirebaseFirestore.DefaultInstance;

                registerAuthEvent();
                AuthStateChanged(this, null);
                Debug.Log("Firebase 초기화 성공");
            }
            else
            {
                Debug.LogErrorFormat("Firebase 초기화 실패 : {0}", dependencyStatus);
            }
        });
    }

    #region 로그인, 계정관리
    //회원가입
    public void CreateID(Tuple<string, string> idPw)
    {
        auth.CreateUserWithEmailAndPasswordAsync(idPw.Item1, idPw.Item2).ContinueWithOnMainThread(task =>
        {
            string message;
            if (task.IsCanceled)
            {
                Debug.LogWarning("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Firebase.FirebaseException e
                = task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;

                if (e != null)
                {
                    Firebase.Auth.AuthError errorCode = (Firebase.Auth.AuthError)e.ErrorCode;

                    switch (errorCode)
                    {
                        case Firebase.Auth.AuthError.EmailAlreadyInUse:
                            {
                                message = "이미 존재하는 계정입니다";
                                createFailedEvent?.Invoke(message, false);
                            }
                            break;
                        case Firebase.Auth.AuthError.InvalidEmail:
                            {
                                message = "올바른 이메일 주소를 입력하세요";
                                createFailedEvent?.Invoke(message, false);
                            }
                            break;
                        case Firebase.Auth.AuthError.WeakPassword:
                            {
                                message = "최소 6자 이상";
                                createFailedEvent?.Invoke(message, false);
                            }
                            break;
                        case Firebase.Auth.AuthError.MissingEmail:
                            {
                                message = "이메일 칸이 비어 있습니다.";
                                createFailedEvent?.Invoke(message, false);
                            }
                            break;
                        case Firebase.Auth.AuthError.MissingPassword:
                            {
                                message = "비밀번호 칸이 비어 있습니다.";
                                createFailedEvent?.Invoke(message, false);
                            }
                            break;
                    }
                }
            }
            else if (task.IsCompleted)
            {
                //Firebase user has been created.
                AuthResult result = task.Result;
                Debug.LogFormat("Firebase user created successfully : {0} ({1})",
                    result.User.DisplayName, result.User.UserId);

                UID = result.User.UserId;

                CreateUserData(result); //파이어베이스 데이터 생성

                message = "계정 생성 성공";
                createFailedEvent?.Invoke(message, true);
            }

        });
    }

    //기존 사용자 로그인
    public void Login(Tuple<string, string> idPw)
    {
        auth.SignInWithEmailAndPasswordAsync(idPw.Item1, idPw.Item2).ContinueWithOnMainThread(task =>
        {
            string message = "";
            if (task.IsCanceled)
            {
                Debug.LogWarning("SignInWithEmailAndPasswordAsync was cancled.");
                return;
            }
            if (task.IsFaulted)
            {
                Firebase.FirebaseException e
                = task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;


                if (e != null)
                {
                    Firebase.Auth.AuthError errorCode = (Firebase.Auth.AuthError)e.ErrorCode;

                    switch (errorCode)
                    {
                        case Firebase.Auth.AuthError.InvalidEmail:
                            {
                                message = "이메일 형식이 올바르지 않습니다.";
                                inputFailedEvent?.Invoke(message, false);
                            }
                            break;
                        case Firebase.Auth.AuthError.WrongPassword:
                            {
                                message = "비밀번호가 틀렸습니다";
                                inputFailedEvent?.Invoke(message, false);
                            }
                            break;
                        case Firebase.Auth.AuthError.UserNotFound:
                            {
                                message = "존재하지 않는 계정입니다.";
                                inputFailedEvent?.Invoke(message, false);
                            }
                            break;
                        default:
                            {
                                message = "로그인 오류: " + errorCode.ToString();
                                inputFailedEvent?.Invoke(message, false);
                            }
                            break;
                    }
                }

                Debug.LogWarning(message);
            }
            if (task.IsCompleted)
            {
                Firebase.Auth.AuthResult result = task.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                    result.User.DisplayName, result.User.UserId);

                UID = result.User.UserId;

                GetUserData(result.User.UserId);

                message = "로그인 성공";
                inputFailedEvent?.Invoke(message, true);
            }
        });
    }

    //인증 상태 변경 이벤트 핸들러 설정 및 사용자 데이터 가져오기
    private void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            /*
            1. IsValid()가 필요한 이유 (C++와 C#의 차이) 파이어베이스 유니티 SDK는 내부적으로
            **C++**로 작성된 엔진을 **C#**에서 쓸 수 있게 연결(Wrapper)한 구조입니다.

            null 체크: "비어 있는가?"만 확인합니다.

            IsValid() 체크: "비어 있지는 않은데, 혹시 내부 엔진(C++)에서 이미 파괴되었거나
            연결이 끊긴 '껍데기' 상태는 아닌가?"를 확인합니다. 비유하자면, null 체크는
            **"내 손에 핸드폰이 있는가?"**를 보는 것이고, IsValid()는 **"그 핸드폰이
            *고장 나지 않고 전원이 켜지는가?"**를 확인하는 과정이라고 보시면 됩니다. 
            */
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null
                && auth.CurrentUser.IsValid();

            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }

            user = auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
                //Null Coalescing Operator / 널 병합 연산자
                displayName = user.DisplayName ?? "";
                emailAddress = user.Email ?? "";
                uid = user.UserId ?? "";
                //photoUrl = user.PhotoUrl ?? "";
            }
        }
    }

    //유저 데이터 생성
    private void CreateUserData(AuthResult result)
    {
        DocumentReference documentReference = db.Collection("UserData").Document(result.User.UserId);
        Dictionary<string, object> initialization = new Dictionary<string, object>
        {
            { "level", 1 },
            { "Exp", 0 }
        };

        Dictionary<string, object> itemList = new Dictionary<string, object>
        {
            { "itemName", null },
            { "count", 0 },
            { "enhancementLevel", 0 }
        };

        List<object> inventory = new List<object>
        {
            itemList
        };

        Dictionary<string, object> equipment = new Dictionary<string, object>
        {
            { "Weapon", null }, //enum EquipSlot list
            { "Armor", null },
            { "Ring", null },
            { "Gloves", null },
            { "Shoes", null }
        };

        Dictionary<string, object> cost = new Dictionary<string, object>
        {
            { "gold", 1000 }, //골드
            { "energe", 60 } //행동력
        };

        Dictionary<string, object> user = new Dictionary<string, object>
        {
            { "User", initialization }, //Dictionary
            { "Inventory", inventory }, //List
            { "Equipment", equipment }, //Dictionary
            { "Cost", cost }            //Dictionary
        };

        documentReference.SetAsync(user).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.IsFaulted)
            {
                Debug.Log("Firebase: 유저 데이터 생성 성공");
            }
            else
            {
                Debug.LogError("Firebase: 유저데이터 생성 실패");
            }
        });
    }
    #endregion

    #region 유저 데이터관리

    public void GetUserData(string uid)
    {
        CollectionReference userRef = db.Collection("UserData");
        userRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.IsFaulted)
            {
                QuerySnapshot snapshot = task.Result;
                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    if (document.Id == uid)
                    {
                        Dictionary<string, object> documentDictionary = document.ToDictionary();
                        RefreshUserData(documentDictionary);
                        break;
                    }
                }
            }
            else
            {
                Debug.Log("GetSnapshotAsync failed");
            }
        });
    }

    private void RefreshUserData(Dictionary<string, object> uid)
    {
        foreach (KeyValuePair<string, object> user in uid)
        {
            if (user.Key == "User")
            {
                Dictionary<string, object> userDetail = user.Value as Dictionary<string, object>;
                if (userDetail != null)
                {
                    userLevel = System.Convert.ToInt32(userDetail["level"]);
                    userCurrentExp = System.Convert.ToInt32(userDetail["Exp"]);
                }
                else
                {
                    Debug.LogWarning("User Data Refresh Failed");
                }
            }
            else if (user.Key == "Inventory")
            {
                List<object> inventory = user.Value as List<object>;
                if (inventory != null)
                {
                    for (int i = 0; i < inventory.Count; i++)
                    {
                        Dictionary<string, object> item = inventory[i] as Dictionary<string, object>;
                        LoadInventory(item);
                    }
                }
                else
                {
                    Debug.LogWarning("Inventory Data Refresh Failed");
                }
            }
            else if (user.Key == "Equipment")
            {
                Dictionary<string, object> equipment = user.Value as Dictionary<string, object>;
                if (equipment != null)
                {
                    foreach (KeyValuePair<string, object> equip in equipment)
                    {
                        string key = equip.Key; //딕셔너리에 들어갈 키 값 = EquipSlot enum List
                        Dictionary<string, object> equipcast = equip.Value as Dictionary<string, object>;
                        //equipcast = { "Weapon", Dictionary }
                        LoadEquipment(equipcast, key);
                    }
                }
                else
                {
                    Debug.LogWarning("Equipment Data Refresh Failed");
                }
            }
            else if (user.Key == "cost")
            {
                Dictionary<string, object> cost = user.Value as Dictionary<string, object>;
                if (cost != null)
                {

                }
                else
                {
                    Debug.LogWarning("Cost Data Refresh Failed");
                }
            }
            
        }
    }

    //get=ture 아이템 추가 || get=false 아이템 제거
    public void FirebaseRefreshItem(string uid, bool get)
    {
        if (get)
        {
            //db.Collection("UserData").Document(uid).
        }
        else if (!get)
        {

        }
    }

    //저장용 데이터 -> 인벤토리 아이템 변환
    private void LoadInventory(Dictionary<string, object> serverData)
    {
        string itemName = serverData["itemName"].ToString();
        int count = System.Convert.ToInt32(serverData["count"]);
        int enhancementLevel = System.Convert.ToInt32(serverData["enhancementLevel"]);

        ItemSO foundSO = StageManager.Instance.GetItemByID(itemName);

        if (foundSO != null)
        {
            InventoryItem item = new InventoryItem();
            item.item = foundSO;
            item.count = count;
            item.enhancementLevel = enhancementLevel;

            userInventory.Add(item); //여기에 담아뒀다가 로비씬 활성화 될 때 Init문에 메서드 던져서 넘기기
        }
        else
        {
            Debug.LogError($"아이템 로드 실패: {itemName}에 해당하는 SO를 찾을 수 없습니다.");
        }
    }

    //인벤토리 채워넣기 : InventoryManager.cs
    public void PushItemList(List<InventoryItem> inventory)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            InventoryItem itemcast = userInventory[i] as InventoryItem;
            if (itemcast != null)
            {
                inventory.Add(itemcast);
            }
            else
            {
                Debug.LogWarningFormat("PushItemList {0} casting Failed", itemcast.item.itemName);
            }
        }
    }

    //인벤토리 아이템 -> 저장용 데이터 변환 -> UpdateAsync
    public void SaveItem(InventoryItem item, string uid)
    {
        Dictionary<string, object> itemCast = new Dictionary<string, object>
        {
            { "itemName", item.item.itemName },
            { "count", item.count },
            { "enhancementLevel", item.enhancementLevel }
        };

        userInventory.Add(itemCast);

        db.Collection("UserData").Document(uid).UpdateAsync("Inventory", userInventory);
    }

    //인벤토리 -> 저장용 데이터 변환 -> UpdateAsync
    public void SaveInventory(List<InventoryItem> inventory)
    {
        userInventory = new List<object>(); //초기화

        for (int i = 0; i < inventory.Count; i++)
        {
            Dictionary<string, object> itemcast = new Dictionary<string, object>
            {
                { "itemName", inventory[i].item.itemName },
                { "count", inventory[i].count },
                { "enhancementLevel", inventory[i].enhancementLevel }
            };

            userInventory.Add(itemcast);
            //UpdateAsync : 키 값이 같을 경우 덮어씌운다, 키 값이 없을 경우 새로 생성
            db.Collection("UserData").Document(uid).UpdateAsync("Inventory", userInventory);
        }
    }

    //저장용 데이터 -> 장비템 변환
    private void LoadEquipment(Dictionary<string, object> serverData, string key)
    {
        string itemName = serverData["itemName"].ToString();
        int count = System.Convert.ToInt32(serverData["count"]);
        int enhancementLevel = System.Convert.ToInt32(serverData["enhancementLevel"]);

        ItemSO foundSO = StageManager.Instance.GetItemByID(itemName);

        if (foundSO != null)
        {
            InventoryItem item = new InventoryItem();
            item.item = foundSO;
            item.count = count;
            item.enhancementLevel = enhancementLevel;

            userEquipment.Add(key, item);
        }
        else
        {
            Debug.LogError($"장비 로드 실패: {itemName}에 해당하는 SO를 찾을 수 없습니다.");
        }
    }

    //장비아이템 채워넣기 : EquipmentManager.cs
    public void PushEquipment(InventoryItem weapone, InventoryItem shoes, InventoryItem gloves,
        InventoryItem armor, InventoryItem emblem
        )
    {
        List<InventoryItem> equipmentList = new List<InventoryItem>
        {
            weapone, shoes, gloves, armor, emblem
        };

        foreach (KeyValuePair<string, object> equip in userEquipment)
        {
            EquipSlot slot;
            InventoryItem unBoxing = equip.Value as InventoryItem;
            //파이어베이스에는 ScriptableObject는 안올라갈텐데?
            EquipmentSO cast = unBoxing.item as EquipmentSO; // -> 값 할당

            if (Enum.TryParse(equip.Key, true, out slot)) Debug.Log("Enum.TryParse 성공");
            else Debug.LogWarning("PushEquipment Enum.TryParse 형변환 실패");

                switch (slot)
                {
                    case EquipSlot.Weapon:
                        {
                            weapone = unBoxing;
                            EquipmentSO extra = weapone.item as EquipmentSO;
                            extra.equipSlot = slot;
                        }
                        break;
                    case EquipSlot.Shoes:
                        {
                            shoes = unBoxing;
                            EquipmentSO extra = shoes.item as EquipmentSO;
                            extra.equipSlot = slot;
                        }
                        break;
                    case EquipSlot.Gloves:
                        {
                            gloves = unBoxing;
                            EquipmentSO extra = gloves.item as EquipmentSO;
                            extra.equipSlot = slot;
                        }
                        break;
                    case EquipSlot.Armor:
                        {
                            armor = unBoxing;
                            EquipmentSO extra = armor.item as EquipmentSO;
                            extra.equipSlot = slot;
                        }
                        break;
                    case EquipSlot.Ring:
                        {
                            emblem = unBoxing;
                            EquipmentSO extra = emblem.item as EquipmentSO;
                            extra.equipSlot = slot;
                        }
                        break;
                }
        }

    }

    //장비템 -> 저장용 데이터 변환 -> UpdateAsync && 해제, 장착할 때 마다 실행?
    public void SaveEquipment(
        InventoryItem weapone, InventoryItem shoes, InventoryItem gloves,
        InventoryItem armor, InventoryItem emblem, string uid
        )
    {
        List<InventoryItem> equipmentList = new List<InventoryItem>
        {
            weapone, shoes, gloves, armor, emblem
        };

        for (int i = 0; i < equipmentList.Count; i++)
        {
            Dictionary<string, object> equipmentCast = new Dictionary<string, object>
            {
                { "itemName", equipmentList[i].item.itemName },
                { "count", equipmentList[i].count },
                { "enhancementLevel", equipmentList[i].enhancementLevel }
            };

            EquipmentSO cast = (EquipmentSO)equipmentList[i].item;

            db.Collection("UserData").Document(uid)
                .UpdateAsync(cast.equipSlot.ToString(), equipmentCast);
        }
    }

    //유저 레벨, 경험치 -> 저장용 데이터

    #endregion

    //메모리
    public void registerAuthEvent()
    {
        auth.StateChanged += AuthStateChanged;
        Debug.Log("AuthStateChanged 이벤트 등록");
    }

    public void UnregisterAuthEvent()
    {
        auth.StateChanged -= AuthStateChanged;
        Debug.Log("AuthStateChanged 이벤트 해제");
    }
    #endregion
}