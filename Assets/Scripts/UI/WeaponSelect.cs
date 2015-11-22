using UnityEngine;
using System.Collections;

namespace ProcRoom.UI
{
    public class WeaponSelect : MonoBehaviour
    {

        [SerializeField]
        OptionButtonFrame group;

        [SerializeField]
        AbilitySelector weaponsRange;

        [SerializeField]
        AbilitySelector weaponsPrecision;

        [SerializeField]
        AbilitySelector weaponsPower;

        WeaponStats currentWeapon;
        WeaponStats optionalWeapon;

        bool keepCurrent = true;

        static WeaponSelect _instance;

        static WeaponSelect instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<WeaponSelect>();
                return _instance;
            }
        }

        public static void Show(WeaponStats optionalWeapon)
        {
            instance.ShowOptions(optionalWeapon);    
        }

        void ShowOptions(WeaponStats optionalWeapon)
        {
            currentWeapon = Tower.Player.Weapon.Stats;
            this.optionalWeapon = optionalWeapon;
            Time.timeScale = 0;
            transform.GetChild(0).gameObject.SetActive(true);
            HandleNewWeaponSelect(group, 0);
        }

        public void Equip()
        {
            if (!keepCurrent)
            {
                Tower.Player.Weapon.SetStats(optionalWeapon);
            }
            transform.GetChild(0).gameObject.SetActive(false);
            Time.timeScale = 1;
        }

        void OnEnable()
        {
            OptionButtonFrame.OnSelectAction += HandleNewWeaponSelect;
        }

        void OnDisable()
        {
            OptionButtonFrame.OnSelectAction -= HandleNewWeaponSelect;
        }

        private void HandleNewWeaponSelect(OptionButtonFrame group, int selected)
        {
            if (group != this.group)
                return;

            keepCurrent = selected == 0;
            WeaponStats selectedWeapon;
            if (keepCurrent)
                selectedWeapon = currentWeapon;
            else
                selectedWeapon = optionalWeapon;

            weaponsRange.Value = selectedWeapon.maxRange;
            weaponsPrecision.Value = selectedWeapon.accuracyLossPerTile;
            weaponsPower.Value = selectedWeapon.attack;

        }
    }
}
