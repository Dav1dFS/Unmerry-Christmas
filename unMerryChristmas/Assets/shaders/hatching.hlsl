fixed3 Hatching(float2 _uv, half _intensity)
{
    half3 hatch0 = tex2D(_Hatch0, _uv).rgb;
    half3 hatch1 = tex2D(_Hatch1, _uv).rgb;

    half3 overbright = max(0, _intensity - 1.0);

    half3 weightsA = saturate((_intensity * 6.0) + half3(-0, -1, -2));
    half3 weightsB = saturate((_intensity * 6.0) + half3(-3, -4, -5));

    weightsA.xy -= weightsA.yz;
    weightsA.z -= weightsB.x;
    weightsB.xy -= weightsB.yz;

    hatch0 = hatch0 * weightsA;
    hatch1 = hatch1 * weightsB;

    half3 hatching = overbright + hatch0.r +
    	hatch0.g + hatch0.b +
    	hatch1.r + hatch1.g +
    	hatch1.b;

    return hatching;
}