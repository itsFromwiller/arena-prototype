using Arena.Items;
using UnityEngine;

namespace Arena
{
    public class WornEquipmentView : MonoBehaviour
    {
        public EquipmentSlotItem MainHandSlot;
        public EquipmentSlotItem OffhandHandSlot;
        public EquipmentSlotItem HelmSlot;
        public EquipmentSlotItem ArmorSlot;
        public EquipmentSlotItem CapeSlot;
        public PlayerStatsView PlayerStatsView;

        public void Setup()
        {
            MainHandSlot.Setup(SlotType.OneHand);
            OffhandHandSlot.Setup(SlotType.OffHand);
            HelmSlot.Setup(SlotType.Head);
            ArmorSlot.Setup(SlotType.Body);
            CapeSlot.Setup(SlotType.Cape);
            PlayerStatsView.Setup();
        }
    }
}
