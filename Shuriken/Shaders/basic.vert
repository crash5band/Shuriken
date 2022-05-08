#version 330 core

layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aUV1;
layout (location = 2) in vec4 aColor;

out vec2 uv1;
out vec4 color;

void main()
{
    uv1         = vec2(aUV1.x, 1.0 - aUV1.y);
    color       = aColor;
    gl_Position = vec4(aPos * vec2(2.0, -2.0) + vec2(-1.0, 1.0), 0.0, 1.0);
}