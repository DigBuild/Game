#version 450

layout(binding = 0) uniform UBO {
	mat4 modelView;
	mat4 projection;
};

layout(location = 0) in vec3 pos;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec2 uv;
layout(location = 3) in vec2 bloomUV;
layout(location = 4) in float brightness;

layout(location = 0) out vec2 fragUV;
layout(location = 1) out vec2 fragBloomUV;
layout(location = 2) out vec3 fragNormal;
layout(location = 3) out vec3 fragPosition;
layout(location = 4) out float fragBrightness;

void main() {
    gl_Position = projection * modelView * vec4(pos.xyz, 1.0);
    gl_Position.z = (gl_Position.z + gl_Position.w) / 2.0;
    fragUV = uv;
    fragBloomUV = bloomUV;
	fragNormal = normal;
	fragPosition = (gl_Position.xyz) * vec3(0.5, 0.5, 1) + vec3(0.5, 0.5, 0);
	fragBrightness = brightness;
}
