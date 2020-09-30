using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
//using Excel;
using UnityEngine;

public static class ConfigLoader
{
    const string SkillFilePath = "";

    static Dictionary<int, SkillData> dicSkillDatas = new Dictionary<int, SkillData>();

    public static bool LoadSkillConfig()
    {
        if (string.IsNullOrEmpty(SkillFilePath)) return false;

        //FileStream stream = null;
        //IExcelDataReader excelReader = null;
        bool success = false;
        //try
        //{
        //    stream = File.Open(SkillFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        //    excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

        //    DataTable table = excelReader.AsDataSet().Tables[0];
        //    dicSkillDatas.Clear();
        //    for (int i = 1, length = table.Rows.Count; i < length; ++i)
        //    {
        //        DataRow singleConfig = table.Rows[i];
        //        if (string.IsNullOrEmpty(singleConfig[0].ToString()))
        //            break;
        //        SkillData skill = new SkillData();
        //        string data = singleConfig[3].ToString();
        //        if (string.IsNullOrEmpty(data) || !int.TryParse(singleConfig[3].ToString(), out skill.id))
        //        {
        //            Debug.LogError("技能表第" + i + "行id为空！");
        //            continue;
        //        }
        //        skill.name = singleConfig[4].ToString();

        //        dicSkillDatas[skill.id] = skill;
        //    }
        //    success = true;
        //}
        //catch (Exception e)
        //{
        //    Debug.LogError("读取技能表出错！" + e.Message);
        //}
        //finally
        //{
        //    if (stream != null) stream.Close();
        //    if (excelReader != null) excelReader.Close();
        //}
        return success;
    }
}
