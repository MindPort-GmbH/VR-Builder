using Unity.Netcode;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

public abstract class LockablePropertyNetwork : NetworkBehaviour
{
    protected NetworkVariable<bool> isLocked = new NetworkVariable<bool>();
    protected abstract LockableProperty lockableProperty { get; }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            isLocked.Value = lockableProperty.IsLocked;

            lockableProperty.Locked += OnLocked;
            lockableProperty.Unlocked += OnUnlocked;
        }

        if (IsClient)
        {
            lockableProperty.SetLocked(isLocked.Value);
            isLocked.OnValueChanged += OnLockedValueChanged;
        }
    }

    private void OnLockedValueChanged(bool previousValue, bool newValue)
    {
        if (previousValue == newValue)
        {
            return;
        }

        if (newValue)
        {
            lockableProperty.SetLocked(true);
        }
        else
        {
            lockableProperty.SetLocked(false);
        }
    }

    private void OnLocked(object sender, LockStateChangedEventArgs e)
    {
        if (IsServer)
        {
            isLocked.Value = true;
        }
    }

    private void OnUnlocked(object sender, LockStateChangedEventArgs e)
    {
        if (IsServer)
        {
            isLocked.Value = false;
        }
    }
}
