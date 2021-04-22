#version 450

layout(location = 0) in vec2 pos;

layout(location = 0) out vec2 fragPos;

void main() {
    gl_Position = vec4((pos - vec2(0.5, 0.5)) * 2, 0.0, 1.0);
    fragPos = pos * 2 - 1;
}
