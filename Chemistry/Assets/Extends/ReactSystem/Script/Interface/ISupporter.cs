using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ReactSystem
{
    public interface ISupporter: IActiveAble
    {
        List<string> GetSupport();
    }

}
