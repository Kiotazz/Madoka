using UnityEngine.Events;

public class NormalEvent : UnityEvent { }
public class IntEvent : UnityEvent<int> { }
public class StringEvent : UnityEvent<string> { }
public class FloatEvent : UnityEvent<float> { }
public class DoubleEvent : UnityEvent<double> { }
public class BoolEvent : UnityEvent<bool> { }
public class CharEvent : UnityEvent<char> { }
public class LongEvent : UnityEvent<long> { }
public class CharacterEvent : UnityEvent<Character> { }
public class BattleTeamEvent : UnityEvent<BattleTeam> { }
public class InteractiveObjEvent : UnityEvent<InteractiveObj> { }
public class Vector2Event : UnityEvent<UnityEngine.Vector2> { }
public class Vector3Event : UnityEvent<UnityEngine.Vector3> { }