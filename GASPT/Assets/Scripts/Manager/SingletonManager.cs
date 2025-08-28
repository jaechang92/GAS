using UnityEngine;

/// <summary>
/// ��� �Ŵ����� �⺻ Ŭ����
/// </summary>
public abstract class SingletonManager<T> : MonoBehaviour where T : MonoBehaviour
{
    private static bool _quitting = false;

    private void OnApplicationQuit()
    {
        _quitting = true;
    }

    // �ν��Ͻ� ���� ����
    private static T _instance;

    // �ν��Ͻ� ���� ������Ƽ
    public static T Instance
    {
        get
        {        
            // �� ���� ���̸� null ��ȯ (�߿�!)
            if (_quitting)
            {
                return null;
            }

            if (_instance == null)
            {
                // 1. ������ ã��
                _instance = FindAnyObjectByType<T>();

                // 2. ã�� ���ߴٸ� ���� ����
                if (_instance == null)
                {
                    // �ʱ�ȭ�� ���ų� �ʱ�ȭ���� �ʾҴٸ� ���� ����
                    GameObject obj = new GameObject($"__{typeof(T).Name}");
                    _instance = obj.AddComponent<T>();

                    Debug.Log($"{typeof(T).Name} �Ŵ��� �ڵ� ������");

                    // �ʱ�ȭ �޼��� ȣ��
                    (_instance as SingletonManager<T>)?.OnCreated();

                    // �� ��ȯ �ÿ��� ����
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

    // ���� �� �ʱ�ȭ (�ʿ� �� �������̵�)
    protected virtual void OnCreated()
    {
        // �ڵ� ���� �� �ʿ��� �ʱ�ȭ �ڵ�
    }

    // ���� �ν��Ͻ� ����
    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
        }
        else if (_instance != this)
        {
            Debug.LogWarning($"�̹� {typeof(T).Name} �ν��Ͻ��� �����մϴ�. �ߺ� ������Ʈ ���ŵ�.");
            Destroy(gameObject);
        }
    }

    // �ν��Ͻ� �Ҹ� �� ���� ����
    protected virtual void OnDestroy()
    {
        // ���� ���� �ƴ� ���� �ν��Ͻ� ���� ����
        if (!_quitting && _instance == this)
        {
            _instance = null;
        }
    }
}