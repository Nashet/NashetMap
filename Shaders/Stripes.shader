Shader "Custom/StripedShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _StripeColor1 ("Stripe Color 1", Color) = (1, 1, 1, 1)
        _StripeColor2 ("Stripe Color 2", Color) = (0, 0, 0, 1)
        _StripeWidth ("Stripe Width", Range(0.0001, 10)) = 0.01
        _StripeAngle ("Stripe Angle", Range(-180, 180)) = 45
    }
 
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM
        #pragma surface surf Lambert
        
        struct Input
        {
            float2 uv_MainTex;
        };
        
        sampler2D _MainTex;
        fixed4 _StripeColor1;
        fixed4 _StripeColor2;
        float _StripeWidth;
        float _StripeAngle;
 
        void surf (Input IN, inout SurfaceOutput o)
        {
            // Rotate the UV coordinates based on the stripe angle
            float2 rotatedUV = IN.uv_MainTex;
            rotatedUV = float2(rotatedUV.x * cos(_StripeAngle) - rotatedUV.y * sin(_StripeAngle),
                               rotatedUV.x * sin(_StripeAngle) + rotatedUV.y * cos(_StripeAngle));
            
            // Calculate the position along the inclined stripes
            float stripePos = frac(rotatedUV.x / _StripeWidth);
            
            // Alternate between stripe colors
            fixed4 stripeColor = (stripePos < 0.5) ? _StripeColor1 : _StripeColor2;
            
            // Apply the stripe effect
            o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * stripeColor.rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}