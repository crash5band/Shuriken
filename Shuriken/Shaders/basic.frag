#version 330 core

in vec2 uv1;
in vec4 color;

out vec4 fragColor;

uniform sampler2D tex;
uniform bool hasTexture;

void main()
{
    fragColor = color;
    
    if (hasTexture)
        fragColor *= texture(tex, uv1);
}
