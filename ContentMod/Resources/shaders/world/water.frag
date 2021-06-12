#version 450
#extension GL_ARB_separate_shader_objects : enable

layout(set = 1, binding = 0) uniform UBO {
	float timeOfDay;
};

layout(set = 2, binding = 1) uniform sampler2D tex;

layout(location = 0) in vec3 fragNormal;
layout(location = 1) in vec3 fragPosition;

layout(location = 0) out vec4 outColor;
layout(location = 1) out vec4 outBloomColor;
layout(location = 2) out vec4 outWater;
layout(location = 3) out vec4 outNormal;
layout(location = 4) out vec4 outPosition;

void main() {
    outColor = outBloomColor = outNormal = outPosition = vec4(0);
    outWater = vec4(1 - fragPosition.z, 1, timeOfDay, 1.0);
}
