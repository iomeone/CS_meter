/*
    Data structure to store training results from demos

    For normalization, this data structure assumes
    team_ct is always counter-terrorist and team_t is
    always terrorist. This provides consistency in
    the training data.
*/

using DemoInfo;
using System;
using System.Collections.Generic;
using System.Reflection;

public class TrainingResult
{
    private static Dictionary<string, PropertyInfo> propertyInfoCache = new Dictionary<string, PropertyInfo>();

    /**********************************
    ** Label, extracted for training **
    **********************************/
    public bool team_ct_wins_round { get; set; } // If true, team_ct wins. If false, team_t wins

    /**************************
    ** Non-player dimensions **
    **************************/
    public string map_id { get; set; }
    public float elapsed_since_bombplant { get; set; }
    public char bombplant_site { get; set; }
    public int round_number { get; set; }
    public int round_of_half { get; set; }
    public int rounds_per_half { get; set; }

    /******************************
    ** Player dimensions, team 1 **
    ******************************/
    public bool team_ct_player1_is_alive { get; set; }
    public EquipmentElement team_ct_player1_equipped_weapon { get; set; }
    public bool team_ct_player1_has_helmet { get; set; }
    public bool team_ct_player1_has_kevlar { get; set; }
    public int team_ct_player1_hp { get; set; }
    public int team_ct_player1_equipment_value { get; set; }
    public bool team_ct_player1_has_defuse_kit { get; set; }

    public bool team_ct_player2_is_alive { get; set; }
    public EquipmentElement team_ct_player2_equipped_weapon { get; set; }
    public bool team_ct_player2_has_helmet { get; set; }
    public bool team_ct_player2_has_kevlar { get; set; }
    public int team_ct_player2_hp { get; set; }
    public int team_ct_player2_equipment_value { get; set; }
    public bool team_ct_player2_has_defuse_kit { get; set; }

    public bool team_ct_player3_is_alive { get; set; }
    public EquipmentElement team_ct_player3_equipped_weapon { get; set; }
    public bool team_ct_player3_has_helmet { get; set; }
    public bool team_ct_player3_has_kevlar { get; set; }
    public int team_ct_player3_hp { get; set; }
    public int team_ct_player3_equipment_value { get; set; }
    public bool team_ct_player3_has_defuse_kit { get; set; }

    public bool team_ct_player4_is_alive { get; set; }
    public EquipmentElement team_ct_player4_equipped_weapon { get; set; }
    public bool team_ct_player4_has_helmet { get; set; }
    public bool team_ct_player4_has_kevlar { get; set; }
    public int team_ct_player4_hp { get; set; }
    public int team_ct_player4_equipment_value { get; set; }
    public bool team_ct_player4_has_defuse_kit { get; set; }

    public bool team_ct_player5_is_alive { get; set; }
    public EquipmentElement team_ct_player5_equipped_weapon { get; set; }
    public bool team_ct_player5_has_helmet { get; set; }
    public bool team_ct_player5_has_kevlar { get; set; }
    public int team_ct_player5_hp { get; set; }
    public int team_ct_player5_equipment_value { get; set; }
    public bool team_ct_player5_has_defuse_kit { get; set; }

    /******************************
    ** Player dimensions, team 1 **
    ******************************/
    public bool team_t_player1_is_alive { get; set; }
    public EquipmentElement team_t_player1_equipped_weapon { get; set; }
    public bool team_t_player1_has_helmet { get; set; }
    public bool team_t_player1_has_kevlar { get; set; }
    public int team_t_player1_hp { get; set; }
    public int team_t_player1_equipment_value { get; set; }

    public bool team_t_player2_is_alive { get; set; }
    public EquipmentElement team_t_player2_equipped_weapon { get; set; }
    public bool team_t_player2_has_helmet { get; set; }
    public bool team_t_player2_has_kevlar { get; set; }
    public int team_t_player2_hp { get; set; }
    public int team_t_player2_equipment_value { get; set; }

    public bool team_t_player3_is_alive { get; set; }
    public EquipmentElement team_t_player3_equipped_weapon { get; set; }
    public bool team_t_player3_has_helmet { get; set; }
    public bool team_t_player3_has_kevlar { get; set; }
    public int team_t_player3_hp { get; set; }
    public int team_t_player3_equipment_value { get; set; }

    public bool team_t_player4_is_alive { get; set; }
    public EquipmentElement team_t_player4_equipped_weapon { get; set; }
    public bool team_t_player4_has_helmet { get; set; }
    public bool team_t_player4_has_kevlar { get; set; }
    public int team_t_player4_hp { get; set; }
    public int team_t_player4_equipment_value { get; set; }

    public bool team_t_player5_is_alive { get; set; }
    public EquipmentElement team_t_player5_equipped_weapon { get; set; }
    public bool team_t_player5_has_helmet { get; set; }
    public bool team_t_player5_has_kevlar { get; set; }
    public int team_t_player5_hp { get; set; }
    public int team_t_player5_equipment_value { get; set; }

    public void SetPlayerIsAlive(Team team, int player, bool isAlive)
        => SetGenericPlayerValue("is_alive", team, player, isAlive);

    public void SetPlayerEquippedWeapon(Team team, int player, EquipmentElement equippedWeapon)
        => SetGenericPlayerValue("equipped_weapon", team, player, equippedWeapon);

    public void SetPlayerHasHelmet(Team team, int player, bool hasHelmet)
        => SetGenericPlayerValue("has_helmet", team, player, hasHelmet);

    public void SetPlayerHasKevlar(Team team, int player, bool hasKevlar)
        => SetGenericPlayerValue("has_kevlar", team, player, hasKevlar);

    public void SetPlayerHP(Team team, int player, int hp)
        => SetGenericPlayerValue("hp", team, player, hp);

    public void SetPlayerEquipmentValue(Team team, int player, int equipementValue)
        => SetGenericPlayerValue("equipment_value", team, player, equipementValue);

    public void SetPlayerHasDefuseKit(int player, bool hasKit)
        => SetGenericPlayerValue("has_defuse_kit", Team.CounterTerrorist, player, hasKit);

    private void SetGenericPlayerValue<T>(string varSuffix, Team team, int player, T value)
    {
        string fullPropName = $"team_{(team == Team.CounterTerrorist ? "ct" : "t")}_player{player}_{varSuffix}";
        var propInfo = GetPropertyInfo(fullPropName);
        propInfo.SetValue(this, value);
    }

    private static PropertyInfo GetPropertyInfo(string name)
    {
        if (!propertyInfoCache.ContainsKey(name))
        {
            var propResult = typeof(TrainingResult).GetProperty(name);

            if (propResult == null)
            {
                throw new ArgumentException(nameof(name));
            }

            propertyInfoCache.Add(name, propResult);
        }

        return propertyInfoCache[name];
    }
}