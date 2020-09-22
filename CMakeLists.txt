cmake_minimum_required(VERSION 3.17)
project(SFMLTest)

set(SFML_DIR "D:/SFML-2.5.1/lib/cmake/SFML")
set(CMAKE_MODULE_PATH "${CMAKE_CURRENT_LIST_DIR}/cmake_modules")
find_package(SFML  COMPONENTS audio graphics window system network)

add_executable(SFMLTest main.cpp Game.cpp Game.h FallingSand.h)

if (SFML_FOUND)
    include_directories(D:/SFML-2.5.1/include)
    target_link_libraries(${PROJECT_NAME} sfml-audio sfml-graphics sfml-window sfml-system sfml-network)
endif ()
