#version 450

layout(binding = 0) uniform UBO {
	mat4 modelView;
	mat4 projection;
};

layout(location = 0) in vec2 pos;
layout(location = 1) in vec2 uv;
layout(location = 2) in vec4 color;

layout(location = 0) out vec2 fragUV;
layout(location = 1) out vec4 fragColor;

void main() {
    gl_Position = projection * modelView * vec4(pos, 0.0, 1.0);
    gl_Position.z = (gl_Position.z + gl_Position.w) / 2.0;
    fragUV = uv;
    fragColor = color;
}
