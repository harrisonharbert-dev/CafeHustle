// Versão simplificada e mais robusta do shader "Sempre por Cima"
Shader"Unlit/AlwaysOnTopShader"
{
    // Propriedades que aparecerão no Inspector do Material
    Properties
    {
        // A textura do nosso ícone
        _MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
        // A cor do componente Image da UI, para que possamos mudar a cor do ícone se quisermos
        _Color ("Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        // Tags para garantir a ordem de renderização e transparência corretas
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }

        Pass
        {
            // --- A LÓGICA PRINCIPAL E SIMPLIFICADA ---

            // Ignora o teste de profundidade. Sempre renderiza.
ZTest Always

            // Habilita a transparência
Blend SrcAlpha
OneMinusSrcAlpha

            // Não escreve no Z-Buffer (importante para transparência)
            ZWrite
Off

            // Não remove polígonos traseiros (bom para planos simples)
            Cull
Off

            // Combina a cor do componente Image com a textura
            SetTexture[_MainTex]
{
    ConstantColor[_Color]
                Combine
    Texture * Constant

}
        }
    }
}