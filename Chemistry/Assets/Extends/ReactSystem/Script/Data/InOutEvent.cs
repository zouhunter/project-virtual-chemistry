using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace ReactSystem
{
    [System.Serializable]
    public class Port
    {
        public int id;//导入的口
        public bool active;//接口开关
        public string[] supportTypes = new string[0];//导入的类型
        public string closeinfomation;//关闭说明
    }

    [System.Serializable]
    public class Equation
    {
        public string[] intypes = new string[0];//反应物类型
        public string[] outtypes = new string[0];//生成的类型
        public float interactTime = 3;//反应时间
        public string[] conditions = new string[0];//反应条件
        public string illustrate;//反应说明
    }

    /// <summary>
    /// 化学反应池
    /// </summary>
    public class InteractPool
    {
        //新元素生成事件
        public UnityAction<string> onNewElementGenerat;
        public UnityAction<string> onEquationActive;
        public UnityAction onAllEquationComplete;
        //元素列表(假定反应物始终存在)
        private List<string> elements = new List<string>();
        //反应条件
        private List<string> conditions = new List<string>();
        //方程式
        private List<Equation> equations = new List<Equation>();
        //激活的方程式
        private Queue<Equation> activeEquations = new Queue<Equation>();
        //完成的(忽略反应过的)
        private List<Equation> completedEquations = new List<Equation>();
        private bool inRact;
        public InteractPool(List<Equation> equations)
        {
            this.equations = equations;
        }

        public IEnumerator LunchInteractPool()
        {
            TryActiveEquation();

            while (completedEquations.Count < equations.Count)
            {
                yield return new WaitForFixedUpdate();//受时间影响可以暂停
                if (activeEquations.Count > 0)
                {
                    var equation = activeEquations.Dequeue();
                    yield return new WaitForSeconds(equation.interactTime);
                    if(onEquationActive != null) onEquationActive.Invoke(equation.illustrate);
                    var outElements = equation.outtypes;
                    foreach (var ele in outElements){
                        AddNewElement(ele);
                    }
                    if (!completedEquations.Contains(equation)) completedEquations.Add(equation);
                }
            }

            if (onAllEquationComplete != null) onAllEquationComplete.Invoke();
        }
        public void AddElements(string element)
        {
            if(AddNewElement(element)) {
                TryActiveEquation();
            }
        }

        public void AddConditions(string condition)
        {
            if (!conditions.Contains(condition))
            {
                conditions.Add(condition);
                TryActiveEquation();
            }
        }

        private void TryActiveEquation()
        {
            foreach (var equation in equations)
            {
                if (!completedEquations.Contains(equation))
                {
                    bool haveElement = true;
                    foreach (var ele in equation.intypes)
                    {
                        haveElement &= elements.Contains(ele);
                    }

                    if (haveElement)
                    {
                        bool haveCondition = true;
                        foreach (var col in equation.conditions)
                        {
                            haveCondition &= conditions.Contains(col);
                        }
                        if (haveCondition)
                        {
                            activeEquations.Enqueue(equation);
                        }
                    }
                }

            }
        }

        private bool AddNewElement(string ele)
        {
            if (!elements.Contains(ele))
            {
                elements.Add(ele);
                if (onNewElementGenerat != null) onNewElementGenerat.Invoke(ele);
                return true;
            }
            return false;
        }

    }
}